# Quiz Import and Export — Backend Implementation Plan

## 1. Overview

This feature adds two endpoints to the existing `QuizzesController`:
- `GET /api/quizzes/{id}/export` — produces a ZIP archive containing `index.yaml` that conforms to `quiz.schema.json`
- `POST /api/quizzes/import` — accepts a multipart ZIP upload and creates a full quiz with all its questions

The implementation follows the existing clean-architecture pattern (Controller → Service → Repository), uses the existing `IFileStorage` abstraction for all file I/O (temp folders, zip/unzip), and reuses the polymorphic `QuizQuestion.QuestionType` JSON system already in place.

---

## 2. Dependencies to Add

Add one NuGet package to `Chik.Exams.csproj`:

```xml
<PackageReference Include="YamlDotNet" Version="16.*" />
```

`System.IO.Compression`, `Newtonsoft.Json`, and `ICSharpCode.SharpZipLib` are already referenced. `IFileStorage` is already registered in DI.

---

## 3. New Files

| Path | Purpose |
|---|---|
| `src/Modules/Quiz/Import/QuizYamlDocument.cs` | POCOs matching `quiz.schema.json` — used by YamlDotNet for serialization and deserialization |
| `src/Modules/Quiz/Import/QuizPortMapper.cs` | Bidirectional mapping between `QuizYamlDocument` and internal `QuizQuestion.QuestionType` JSON format |
| `src/Modules/Quiz/Import/IQuizImportExportService.cs` | Service interface |
| `src/Modules/Quiz/Import/QuizImportExportService.cs` | Service implementation — orchestrates file I/O via `IFileStorage`, YAML parsing, and DB operations |

---

## 4. Files to Modify

| Path | Change |
|---|---|
| `api/Controllers/QuizzesController.cs` | Add `Export` and `Import` action methods; inject `IQuizImportExportService` |
| `src/Modules/Quiz/QuizExtensions.cs` | Register `IQuizImportExportService` via `TrackScoped` |

---

## 5. Step-by-Step Implementation

### 5.1 `src/Modules/Quiz/Import/QuizYamlDocument.cs`

POCOs that YamlDotNet can deserialize/serialize. Property names are PascalCase; the YamlDotNet `CamelCaseNamingConvention` maps them to the camelCase YAML keys.

```csharp
namespace Chik.Exams;

/// <summary>Root document matching quiz.schema.json.</summary>
public class QuizYamlDocument
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>HH:MM:SS string or null.</summary>
    public string? Duration { get; set; }
    public List<QuizYamlQuestion> Questions { get; set; } = [];
}

public class QuizYamlQuestion
{
    public string Prompt { get; set; } = string.Empty;
    /// <summary>One of: multiple-choice, single-choice, true-or-false, fill-in-the-blank, essay, short-answer</summary>
    public string Type { get; set; } = string.Empty;
    public int Score { get; set; }
    /// <summary>
    /// Kept as Dictionary so we can handle polymorphic properties without a custom YamlDotNet converter.
    /// Empty YAML {} block deserializes to an empty dictionary (not null).
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
}
```

---

### 5.2 `src/Modules/Quiz/Import/QuizPortMapper.cs`

Pure static class. All type-specific mapping knowledge lives here.

```csharp
namespace Chik.Exams;

public static class QuizPortMapper
{
    // ...
}
```

#### `ToYaml(QuizQuestion question) → QuizYamlQuestion`

Called once per question during **export**.

1. Deserialize `question.Properties` (the raw JSON string) using `Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(question.Properties)`.
2. Switch on `question.Type?.Name` (or the `type` discriminator inside the deserialized dict):
   - `"multiple-choice"` / `"single-choice"`:
     - Extract `options` array from the internal dict.
     - Write `Properties = { ["options"] = List<dict { "text", "isCorrect" }> }`.
   - `"true-or-false"`:
     - Read `correctAnswer` from internal dict.
     - Write `Properties = { ["answer"] = correctAnswer }` — **key renames from `correctAnswer` → `answer`**.
   - `"fill-in-the-blank"`, `"essay"`, `"short-answer"`:
     - Write `Properties = {}` (empty dict) — all internal detail is stripped.
3. Return `QuizYamlQuestion { Prompt, Type = typeString, Score, Properties }`.

Do **not** include the internal `"type"` discriminator key inside `Properties` — the schema carries type at the question level.

#### `ToInternalProperties(QuizYamlQuestion yaml) → string`

Called once per question during **import**. Returns the raw JSON string for `QuizQuestion.Properties`.

Switch on `yaml.Type`:

| `yaml.Type` | Input from YAML | Output JSON |
|---|---|---|
| `"multiple-choice"` | `Properties["options"]` → list of `{text, isCorrect}` dicts | `{"type":"multiple-choice","options":[{"text":"...","isCorrect":true}]}` |
| `"single-choice"` | same | `{"type":"single-choice","options":[...]}` |
| `"true-or-false"` | `Properties["answer"]` → bool | `{"type":"true-or-false","correctAnswer":true}` (**key renames `answer` → `correctAnswer`**) |
| `"fill-in-the-blank"` | `{}` (ignored) | `{"type":"fill-in-the-blank","acceptedAnswers":[]}` |
| `"essay"` | `{}` (ignored) | `{"type":"essay","minWords":null,"maxWords":null}` |
| `"short-answer"` | `{}` (ignored) | `{"type":"short-answer","acceptedAnswers":[],"maxLength":null}` |

Use `Newtonsoft.Json.JsonConvert.SerializeObject(...)` for all output.

**YamlDotNet dict casting note:** YamlDotNet deserializes nested YAML mappings as `Dictionary<object, object>`, not `Dictionary<string, object>`. Read options safely:

```csharp
var rawOptions = (List<object>)yaml.Properties!["options"];
var options = rawOptions.Select(o =>
{
    var dict = (Dictionary<object, object>)o;
    return new { text = dict["text"].ToString()!, isCorrect = Convert.ToBoolean(dict["isCorrect"]) };
}).ToList();
```

For `true-or-false`, use `Convert.ToBoolean(yaml.Properties!["answer"])` to handle both `bool` and `string` representations from YamlDotNet.

#### `ToTypeId(string yamlType) → int`

Lookup matching seeded DB values:

```
"multiple-choice"   → 1
"single-choice"     → 2
"fill-in-the-blank" → 3
"essay"             → 4
"short-answer"      → 5
"true-or-false"     → 6
```

Throw `ValidationException($"Unknown question type: '{yamlType}'")` for unrecognised values.

#### `ToDuration(TimeSpan? ts) → string?`

```csharp
ts is null ? null : ts.Value.ToString(@"hh\:mm\:ss")
```

#### `FromDurationString(string? s) → TimeSpan?`

```csharp
if (s is null) return null;
if (!TimeSpan.TryParseExact(s, @"hh\:mm\:ss", null, out var ts))
    throw new ValidationException("'duration' must be in HH:MM:SS format, e.g. '01:30:00'");
return ts;
```

---

### 5.3 `src/Modules/Quiz/Import/IQuizImportExportService.cs`

```csharp
namespace Chik.Exams;

public interface IQuizImportExportService
{
    /// <summary>
    /// Builds a ZIP archive and returns it as a byte array with a suggested file name.
    /// Throws KeyNotFoundException if the quiz does not exist.
    /// Throws UnauthorizedAccessException if the caller cannot access the quiz.
    /// </summary>
    Task<(byte[] ZipBytes, string FileName)> ExportQuiz(Auth auth, long quizId);

    /// <summary>
    /// Parses a ZIP archive from a stream, validates the YAML, creates the quiz and all questions.
    /// Returns the fully-populated Quiz (with Questions).
    /// Throws ValidationException with a descriptive message on any structural error.
    /// </summary>
    Task<Quiz> ImportQuiz(Auth auth, Stream zipStream, long? examinerId);
}
```

---

### 5.4 `src/Modules/Quiz/Import/QuizImportExportService.cs`

**Constructor injection:**

```csharp
internal class QuizImportExportService(
    IQuizService quizService,
    IQuizQuestionRepository questionRepository,
    IFileStorage fileStorage,
    IAuditLogService auditLogService,
    ILogger<QuizImportExportService> logger
) : IQuizImportExportService
```

`IQuizService.Create` is used for the quiz so its audit log fires correctly. Questions are inserted directly via `IQuizQuestionRepository` to avoid N redundant authorization checks in a tight import loop.

#### `ExportQuiz(Auth auth, long quizId)`

1. Call `await quizService.Get(auth, quizId, includeQuestions: true)`. This performs authorization (throws `UnauthorizedAccessException` or returns null). If null, throw `KeyNotFoundException`.
2. Build a `QuizYamlDocument`:
   ```csharp
   var document = new QuizYamlDocument
   {
       Title = quiz.Title,
       Description = quiz.Description,
       Duration = QuizPortMapper.ToDuration(quiz.Duration),
       Questions = quiz.Questions?.Select(QuizPortMapper.ToYaml).ToList() ?? []
   };
   ```
3. Serialize with YamlDotNet:
   ```csharp
   var serializer = new SerializerBuilder()
       .WithNamingConvention(CamelCaseNamingConvention.Instance)
       .Build();
   string yaml = serializer.Serialize(document);
   ```
4. Use `IFileStorage` to write `index.yaml` to a temp export folder, zip it, read back the bytes, then clean up:
   ```csharp
   string tempFolder = $"quizzes/exports/{quizId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
   fileStorage.CreateDirectory(tempFolder);
   
   string yamlPath = $"{tempFolder}/index.yaml";
   using var yamlStream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
   await fileStorage.SaveFileAsync(yamlPath, yamlStream);
   
   string zipPath = await fileStorage.ZipFolderAsync(tempFolder);
   byte[] bytes;
   using (var zipStream = await fileStorage.ReadFileAsync(zipPath))
   using (var ms = new MemoryStream())
   {
       await zipStream.CopyToAsync(ms);
       bytes = ms.ToArray();
   }
   
   await fileStorage.DeleteFolderAsync(tempFolder);
   await fileStorage.DeleteFileAsync(zipPath);
   ```
5. Emit audit log:
   ```csharp
   await auditLogService.Create(auth, new AuditLog.Create<object>(
       $"{nameof(QuizImportExportService)}.{nameof(ExportQuiz)}",
       quizId,
       new { QuizId = quizId }
   ));
   ```
6. Generate safe filename:
   ```csharp
   var safeTitle = Regex.Replace(quiz.Title, @"[^\w\-]", "-").ToLowerInvariant();
   string fileName = $"quiz-{quizId}-{safeTitle}.zip";
   ```
7. Return `(bytes, fileName)`.

#### `ImportQuiz(Auth auth, Stream zipStream, long? examinerId)`

1. Authorization: if `!auth.IsAdmin() && !auth.IsTeacher()`, throw `UnauthorizedAccessException`.
2. Save the uploaded ZIP via `IFileStorage`:
   ```csharp
   string tempZipPath = $"quizzes/imports/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.zip";
   await fileStorage.SaveFileAsync(tempZipPath, zipStream);
   ```
3. Unzip via `IFileStorage`:
   ```csharp
   string extractedFolder;
   try
   {
       extractedFolder = await fileStorage.UnzipAsync(tempZipPath);
   }
   catch (InvalidDataException)
   {
       await fileStorage.DeleteFileAsync(tempZipPath);
       throw new ValidationException("The uploaded file is not a valid ZIP archive");
   }
   ```
4. Read `index.yaml`:
   ```csharp
   string yamlFilePath = Path.Combine(
       Path.GetFileNameWithoutExtension(fileStorage.GetFilePath(tempZipPath)),
       "index.yaml"
   );
   // Simpler: construct the path that UnzipAsync produces (same dir as zip, same name as folder)
   if (!await fileStorage.FileExistsAsync(yamlFilePath))
   {
       await Cleanup(tempZipPath, extractedFolder);
       throw new ValidationException("ZIP archive must contain 'index.yaml' at the root");
   }
   string yamlText = await fileStorage.ReadFileTextAsync(yamlFilePath);
   ```
   
   > **Note on path:** `fileStorage.UnzipAsync(zipFilePath)` returns the extracted folder path (e.g., `{rootPath}/quizzes/imports/12345/`). Read `index.yaml` from `{extractedFolder}/index.yaml` using `fileStorage.ReadFileTextAsync`. Since `ReadFileTextAsync` prepends `_rootPath`, pass the path **relative to `_rootPath`**, i.e., strip `_rootPath` prefix from the returned absolute path. To avoid this complexity, compute the relative `extractedFolder` before passing to `UnzipAsync`, or read using a full path via `File.ReadAllText` directly on the absolute path returned by `UnzipAsync`.
   
   Recommended approach — use `File.ReadAllText` with the absolute path since `UnzipAsync` returns an absolute path:
   ```csharp
   string absoluteYaml = Path.Combine(extractedFolder, "index.yaml");
   if (!File.Exists(absoluteYaml))
   {
       await Cleanup(tempZipPath, extractedFolder);
       throw new ValidationException("ZIP archive must contain 'index.yaml' at the root");
   }
   string yamlText = await File.ReadAllTextAsync(absoluteYaml);
   ```

5. Deserialize with YamlDotNet:
   ```csharp
   var deserializer = new DeserializerBuilder()
       .WithNamingConvention(CamelCaseNamingConvention.Instance)
       .IgnoreUnmatchedProperties()
       .Build();
   QuizYamlDocument document;
   try
   {
       document = deserializer.Deserialize<QuizYamlDocument>(yamlText);
   }
   catch (Exception ex)
   {
       await Cleanup(tempZipPath, extractedFolder);
       throw new ValidationException($"index.yaml is malformed: {ex.Message}");
   }
   ```

6. Validate required fields (throw `ValidationException` on failure):
   - `document.Title` must be non-null/non-empty → `"'title' is required"`
   - `document.Questions` must be non-null with at least one item → `"'questions' must contain at least one question"`
   - For each question at index `i` (1-based in messages):
     - `Prompt` non-empty → `"Question {i}: 'prompt' is required"`
     - `Type` is one of the six valid values → `"Question {i}: unknown type '{type}'"`
     - `Score >= 0` → `"Question {i}: 'score' must be >= 0"`
     - `multiple-choice` / `single-choice`: `Properties` must contain `"options"` key → `"Question {i}: 'properties.options' is required for type '{type}'"`
     - `true-or-false`: `Properties` must contain `"answer"` key → `"Question {i}: 'properties.answer' is required for type 'true-or-false'"`

7. Parse duration: `QuizPortMapper.FromDurationString(document.Duration)` (throws `ValidationException` on format error).

8. Create the quiz:
   ```csharp
   var quiz = await quizService.Create(auth, new Quiz.Create(
       document.Title,
       document.Description ?? string.Empty,
       auth.Id,
       examinerId,
       duration
   ));
   ```

9. Create questions in sequence:
   ```csharp
   var createdQuestions = new List<QuizQuestion>();
   for (int i = 0; i < document.Questions.Count; i++)
   {
       var yamlQ = document.Questions[i];
       int typeId = QuizPortMapper.ToTypeId(yamlQ.Type);
       string propertiesJson = QuizPortMapper.ToInternalProperties(yamlQ);
       var questionDbo = await questionRepository.Create(new QuizQuestion.Create(
           quiz.Id,
           yamlQ.Prompt,
           typeId,
           propertiesJson,
           yamlQ.Score,
           Order: i + 1
       ));
       createdQuestions.Add(questionDbo.ToModel());
   }
   ```

10. Emit audit log:
    ```csharp
    await auditLogService.Create(auth, new AuditLog.Create<object>(
        $"{nameof(QuizImportExportService)}.{nameof(ImportQuiz)}",
        quiz.Id,
        new { QuizId = quiz.Id, QuestionCount = createdQuestions.Count }
    ));
    ```

11. Clean up temp files:
    ```csharp
    await fileStorage.DeleteFileAsync(tempZipPath);
    await fileStorage.DeleteFolderAsync(/* relative path of extractedFolder */);
    ```

12. Return quiz with questions attached:
    ```csharp
    return quiz with { Questions = createdQuestions };
    ```

**Private `Cleanup` helper:**
```csharp
private async Task Cleanup(string zipPath, string? extractedFolder = null)
{
    await fileStorage.DeleteFileAsync(zipPath);
    if (extractedFolder is not null)
        await fileStorage.DeleteFolderAsync(extractedFolder);
}
```

---

### 5.5 `api/Controllers/QuizzesController.cs` — Add Two Actions

Add `IQuizImportExportService _quizImportExportService` to the constructor.

#### Export Action

```csharp
/// <summary>
/// Exports a quiz as a ZIP archive containing index.yaml.
/// Admins can export any quiz; Teachers can only export their own or assigned quizzes.
/// </summary>
[HttpGet("{id:long}/export")]
[AdminOrTeacher]
public async Task<IActionResult> Export(long id, [FromServices] Auth auth)
{
    var (zipBytes, fileName) = await _quizImportExportService.ExportQuiz(auth, id);
    return File(zipBytes, "application/zip", fileName);
}
```

`File(byte[], contentType, fileDownloadName)` sets both `Content-Type` and `Content-Disposition: attachment; filename="..."` automatically.

#### Import Action

```csharp
/// <summary>
/// Imports a quiz from a ZIP archive containing index.yaml.
/// Request: multipart/form-data with 'file' (the .zip) and optional 'examinerId'.
/// Returns the created Quiz with all questions included.
/// </summary>
[HttpPost("import")]
[AdminOrTeacher]
public async Task<ActionResult<Quiz>> Import(
    IFormFile file,
    [FromForm] long? examinerId,
    [FromServices] Auth auth)
{
    if (file is null || file.Length == 0)
        return BadRequest(new { Message = "A ZIP file must be provided in the 'file' field" });

    bool isZip = file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
        || file.ContentType is "application/zip" or "application/octet-stream";
    if (!isZip)
        return BadRequest(new { Message = "Uploaded file must be a ZIP archive" });

    using var stream = file.OpenReadStream();
    var quiz = await _quizImportExportService.ImportQuiz(auth, stream, examinerId);

    _logger.LogInformation("Quiz '{Title}' imported by {Creator}", quiz.Title, auth.Username);
    return CreatedAtAction(nameof(Get), new { id = quiz.Id }, quiz);
}
```

`ValidationException` thrown from the service maps to HTTP 400 via the existing global exception handler.

---

### 5.6 `src/Modules/Quiz/QuizExtensions.cs`

Add one line to the quiz module's DI registration method:

```csharp
services.TrackScoped<IQuizImportExportService, QuizImportExportService>();
```

---

## 6. Data Mapping Reference

### 6.1 Export: Internal JSON → YAML

| Question Type | Internal `Properties` JSON | YAML `properties` block |
|---|---|---|
| `multiple-choice` | `{"type":"multiple-choice","options":[{"text":"A","isCorrect":true}]}` | `options: [{text: A, isCorrect: true}]` |
| `single-choice` | `{"type":"single-choice","options":[...]}` | `options: [...]` |
| `true-or-false` | `{"type":"true-or-false","correctAnswer":true}` | `answer: true` (**`correctAnswer` → `answer`**) |
| `fill-in-the-blank` | `{"type":"fill-in-the-blank","acceptedAnswers":["nucleus"]}` | `{}` (content stripped) |
| `essay` | `{"type":"essay","minWords":50,"maxWords":500}` | `{}` (content stripped) |
| `short-answer` | `{"type":"short-answer","acceptedAnswers":[],"maxLength":100}` | `{}` (content stripped) |

Internal `"type"` discriminator is **never** written into the YAML `properties` block — it lives at the question-level `type` field.

Duration: `TimeSpan?` → `HH:MM:SS` string or omitted if null.

### 6.2 Import: YAML → Internal JSON

| YAML `type` | YAML `properties` | TypeId | Internal JSON stored |
|---|---|---|---|
| `multiple-choice` | `{options:[{text,isCorrect},...]}` | 1 | `{"type":"multiple-choice","options":[{"text":"...","isCorrect":true}]}` |
| `single-choice` | `{options:[{text,isCorrect},...]}` | 2 | `{"type":"single-choice","options":[...]}` |
| `fill-in-the-blank` | `{}` | 3 | `{"type":"fill-in-the-blank","acceptedAnswers":[]}` |
| `essay` | `{}` | 4 | `{"type":"essay","minWords":null,"maxWords":null}` |
| `short-answer` | `{}` | 5 | `{"type":"short-answer","acceptedAnswers":[],"maxLength":null}` |
| `true-or-false` | `{answer: bool}` | 6 | `{"type":"true-or-false","correctAnswer":true}` (**`answer` → `correctAnswer`**) |

---

## 7. Authorization Rules

| Operation | Admin | Teacher |
|---|---|---|
| Export | Any quiz | Only where `CreatorId == auth.Id` OR `ExaminerId == auth.Id` |
| Import | Can import; may specify any `examinerId` | Can import; `CreatorId` is always `auth.Id`; may specify `examinerId` |

For export, authorization is fully delegated to `quizService.Get(auth, quizId, includeQuestions: true)` which already contains the correct teacher check. No duplicate logic needed.

For import, `quizService.Create(auth, ...)` always sets `CreatorId = auth.Id`, which is correct.

---

## 8. Error Handling Strategy

All validation failures throw `System.ComponentModel.DataAnnotations.ValidationException`, which the existing global exception handler maps to HTTP 400.

| Condition | Response |
|---|---|
| Missing `file` field or empty file | 400 — early return in controller |
| Non-ZIP content type | 400 — early return in controller |
| Uploaded bytes are not a valid ZIP | 400 `ValidationException` from service |
| ZIP has no `index.yaml` at root | 400 `ValidationException` |
| YAML parse error | 400 `ValidationException("index.yaml is malformed: ...")` |
| Missing `title` | 400 `ValidationException` |
| Empty `questions` | 400 `ValidationException` |
| Unknown question `type` | 400 `ValidationException("Question {i}: unknown type '...'")`|
| Missing `options` for choice types | 400 `ValidationException("Question {i}: 'properties.options' is required...")` |
| Missing `answer` for true-or-false | 400 `ValidationException("Question {i}: 'properties.answer' is required...")` |
| Invalid `duration` format | 400 `ValidationException` |
| Quiz not found (export) | 404 — via existing `KeyNotFoundException` handler |
| Unauthorized access (export) | 401 — via existing `UnauthorizedAccessException` handler |

Temp files are cleaned up in all error paths (both success and failure) using a `try/finally` or the `Cleanup` helper.

---

## 9. Testing Checklist

### Export
- [ ] All six question types serialize correctly per the mapping table above
- [ ] `true-or-false` exports `answer`, not `correctAnswer`
- [ ] `fill-in-the-blank`, `essay`, `short-answer` export empty `{}` properties
- [ ] `multiple-choice` and `single-choice` export the full `options` array
- [ ] `duration` serializes as `HH:MM:SS`; null duration is omitted
- [ ] Response is `application/zip` with correct `Content-Disposition` header
- [ ] Admin can export any quiz
- [ ] Teacher can export own quiz (`CreatorId == auth.Id`)
- [ ] Teacher can export quiz where they are examiner (`ExaminerId == auth.Id`)
- [ ] Teacher cannot export another teacher's unassigned quiz → 401
- [ ] Non-existent quiz returns 404
- [ ] Exported ZIP is a valid archive that can be opened with standard tools

### Import
- [ ] All six question types import with correct `TypeId` and internal `Properties` JSON
- [ ] `true-or-false` stores `correctAnswer` (not `answer`) in DB after import
- [ ] `fill-in-the-blank` stores `{"type":"fill-in-the-blank","acceptedAnswers":[]}` after import
- [ ] `essay` stores `{"type":"essay","minWords":null,"maxWords":null}` after import
- [ ] `short-answer` stores `{"type":"short-answer","acceptedAnswers":[],"maxLength":null}` after import
- [ ] Question `Order` values are 1-based and match YAML sequence
- [ ] `CreatorId` is always `auth.Id`, regardless of `examinerId`
- [ ] `examinerId` form field is stored on the created quiz
- [ ] Missing `file` field → 400
- [ ] Non-ZIP upload → 400
- [ ] Malformed ZIP bytes → 400
- [ ] ZIP with no `index.yaml` → 400
- [ ] Malformed YAML → 400
- [ ] Missing `title` → 400
- [ ] Empty `questions` → 400
- [ ] Unknown `type` → 400 with question index
- [ ] `multiple-choice` missing `options` → 400
- [ ] `true-or-false` missing `answer` → 400
- [ ] Bad `duration` format → 400
- [ ] Valid import returns 201 with Quiz JSON including all questions
- [ ] Temp files are cleaned up after both successful and failed imports
- [ ] Round-trip: export a quiz → import it → new quiz has identical title/description/duration/question count/types/scores
- [ ] Admin can import; Teacher can import; Student cannot → 401
