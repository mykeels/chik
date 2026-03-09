/**
 * Parser for freetext comic input format
 * 
 * Expected format:
 * ```
 * Friday, January 9, 2026
 * 
 * CHAPTER: 2 Samuel 1
 * 
 * [Main content paragraphs separated by blank lines]
 * 
 * Prayer
 * 
 * [Prayer text]
 * 
 * Questions
 * 
 * 1. [Question]
 *    [Answer]
 * 2. [Question]
 *    [Answer]
 * ...
 * ```
 */

export interface ParsedComicInput {
  date: string;
  dateFormatted: string;
  chapter: string;
  paragraphs: string[];
  questions: Array<{ question: string; answer: string }>;
  prayer: string;
  fullContent: string;
}

/**
 * Parse freetext input into structured comic data
 */
export function parseComicInput(input: string): ParsedComicInput {
  const lines = input.split("\n");

  // Extract date from first line (e.g., "Wednesday, January 7, 2026")
  const dateMatch = lines[0].match(/(\w+),\s+(\w+)\s+(\d+),\s+(\d+)/);
  let dateFormatted = "";
  if (dateMatch) {
    const [, , month, day, year] = dateMatch;
    const monthMap: Record<string, string> = {
      January: "01",
      February: "02",
      March: "03",
      April: "04",
      May: "05",
      June: "06",
      July: "07",
      August: "08",
      September: "09",
      October: "10",
      November: "11",
      December: "12",
    };
    dateFormatted = `${year}-${monthMap[month]}-${day.padStart(2, "0")}`;
  }

  // Extract chapter
  const chapterLine = lines.find((l) => l.startsWith("CHAPTER:"));
  const chapter = chapterLine?.replace("CHAPTER:", "").trim() || "";

  // Split content into sections
  const fullText = input;
  const prayerIndex = fullText.indexOf("Prayer");
  const questionsIndex = fullText.indexOf("Questions");

  // Get main content (between chapter and Prayer section)
  const chapterIndex = fullText.indexOf("CHAPTER:");
  const endOfChapterLine = fullText.indexOf("\n", chapterIndex);
  const mainContent = fullText.slice(endOfChapterLine + 1, prayerIndex).trim();

  // Split into paragraphs (non-empty lines)
  const paragraphs = mainContent
    .split("\n\n")
    .map((p) => p.trim())
    .filter((p) => p.length > 0 && !p.startsWith("*"));

  // Extract prayer
  const prayer = fullText.slice(prayerIndex + 6, questionsIndex).trim();

  // Extract questions with answers
  const questionsSection = fullText.slice(questionsIndex + 9).trim();
  const questionLines = questionsSection.split("\n");
  
  const questions: Array<{ question: string; answer: string }> = [];
  let currentQuestion = "";
  let currentAnswer = "";
  
  for (const line of questionLines) {
    const trimmedLine = line.trim();
    if (!trimmedLine) continue;
    
    // Check if this is a new question (starts with number)
    const questionMatch = trimmedLine.match(/^(\d+)\.\s*(.+)/);
    if (questionMatch) {
      // Save previous Q&A if exists
      if (currentQuestion) {
        questions.push({
          question: currentQuestion,
          answer: currentAnswer.trim(),
        });
      }
      currentQuestion = questionMatch[2];
      currentAnswer = "";
    } else {
      // This is an answer line
      currentAnswer += (currentAnswer ? " " : "") + trimmedLine;
    }
  }
  
  // Don't forget the last Q&A
  if (currentQuestion) {
    questions.push({
      question: currentQuestion,
      answer: currentAnswer.trim(),
    });
  }

  return {
    date: lines[0],
    dateFormatted,
    chapter,
    paragraphs,
    questions,
    prayer,
    fullContent: mainContent,
  };
}

/**
 * Generate a URL-friendly slug from a title
 */
export function generateSlug(title: string): string {
  return title
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, "")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-")
    .trim();
}

/**
 * Generate a title from the chapter
 */
export function generateTitle(chapter: string): string {
  return `${chapter}: A Bible Story`;
}
