import fs from "fs";
import fg from "fast-glob";
import path from "path";
import { CAC } from "cac";
import { execSync } from "child_process";
import { generateZodClientFromOpenAPI } from "openapi-zod-client";
import { oas31 } from "openapi3-ts";

const generateAPIClient = async (swaggerFilePath: string) => {
  const clientOutputDir = path.dirname(swaggerFilePath);
  const openApiDoc = JSON.parse(fs.readFileSync(swaggerFilePath, "utf8"));
  process.env.NEXT_PROJECT_VERSION = openApiDoc.info.version;
  console.log("Generating API Client for: ", swaggerFilePath);
  const templatePath = ["./templates/default.hbs", "../templates/default.hbs"]
    .map((filePath) => path.join(__dirname, filePath))
    .find((filePath) => fs.existsSync(filePath));
  console.log("Using template: ", templatePath);
  const code = await generateZodClientFromOpenAPI({
    openApiDoc,
    distPath: path.join(clientOutputDir, "./index.ts"),
    templatePath,
    options: {
      withAlias: true,
      shouldExportAllTypes: true,
      additionalPropertiesDefaultValue: false,
      complexityThreshold: -1,
    },
  });
  const enumConstants = extractEnumConstants(openApiDoc);
  return code
    .replaceAll(
      "datetime({ offset: true })",
      "datetime({ offset: true, local: true })",
    )
    .replaceAll(
      "ZodiosInstance<TEndpoints>;",
      `ZodiosInstance<TEndpoints>;\n\n${enumConstants}`,
    );
};

const extractEnumConstants = (openApiDoc: oas31.OpenAPIObject) => {
  const schemas = openApiDoc.components?.schemas;
  if (!schemas) return {};
  const enumConstants = {} as Record<string, string>;
  for (const [key, schema] of Object.entries(schemas)) {
    if ("enum" in schema && "x-enum-varnames" in schema) {
      const enumValues = schema.enum as number[];
      const enumNames = schema["x-enum-varnames"] as string[];
      const enumMap = enumNames.reduce(
        (acc, name, index) => {
          acc[name] = enumValues[index];
          return acc;
        },
        {} as Record<string, string | number>,
      );
      enumConstants[key] = `
export const ${key.replaceAll(".", "_")} = {
${enumNames.map((name) => `${name}: ${typeof enumMap[name] === "string" ? `'${enumMap[name]}'` : enumMap[name]}`).join(",\n")}
} as const;
export type ${key.replaceAll(".", "_")}_Values = typeof ${key.replaceAll(".", "_")}[keyof typeof ${key.replaceAll(".", "_")}];
      `;
    }
  }
  return `export namespace enums {
    ${Object.values(enumConstants).join("\n")}
  }`;
};

const generateIndex = (keys: string[], outputDir: string) => {
  const index = `
    import { type ZodiosOptions } from "@zodios/core";

    ${keys
      .map(
        (key) =>
          `import { createApiClient as ${key}, schemas as ${key}Schema } from "./${key}";`,
      )
      .join("\n")}

      export const schemas: {
        ${keys.map((key) => `${key}: typeof ${key}Schema`).join(",\n")}
      } = {
        ${keys.map((key) => `${key}: ${key}Schema`).join(",\n")}
      };

      export const api: {
        ${keys.map((key) => `${key}: typeof ${key}`).join(",\n")}
      } = {
        ${keys.map((key) => `${key}: ${key}`).join(",\n")}
      };

      type Api = {
        ${keys.map((key) => `${key}: ReturnType<typeof ${key}>;`).join("\n")}
      };

    export const createApiClient = (baseUrl: string, consumerName: string, options?: ZodiosOptions): Api => ({
        ${keys.map((key) => `${key}: ${key}(baseUrl, consumerName, options),`).join("\n")}
    })

    export default createApiClient;
    `;

  fs.writeFileSync(path.join(outputDir, "./index.ts"), index);
};

const copyDirSync = (src: string, dest: string) => {
  fs.mkdirSync(dest, { recursive: true });
  for (const item of fs.readdirSync(src)) {
    const srcPath = path.join(src, item);
    const destPath = path.join(dest, item);
    if (fs.lstatSync(srcPath).isDirectory()) {
      copyDirSync(srcPath, destPath);
    }
    if (fs.lstatSync(srcPath).isFile()) {
      fs.copyFileSync(srcPath, destPath);
    }
  }
};

function copyIncludes(outputDir: string) {
  const includesDir = [
    path.join(__dirname, "../src/commands/includes/node"),
    path.join(__dirname, "../src/includes/node"),
    path.join(__dirname, "../src/includes"),
    path.join(__dirname, "./commands/includes/node"),
    path.join(__dirname, "./includes/node"),
    path.join(__dirname, "./includes"),
  ].find((p) => fs.existsSync(p))!;
  console.log("Copying Includes Directory: ", includesDir);
  copyDirSync(includesDir, path.join(outputDir, "./includes"));
}

export const generateNodeSwaggerClient = async (
  swaggerFilePath: string,
  skipIncludes = false,
) => {
  const outputDir = path.dirname(swaggerFilePath);
  const code = await generateAPIClient(swaggerFilePath);
  const refactoredCode = code;
  fs.writeFileSync(path.join(outputDir, `./index.ts`), refactoredCode);
  if (!skipIncludes) {
    copyIncludes(outputDir);
  }
};

export const generateNodeSwaggerClients = async (outputDir: string) => {
  const keys = [] as string[];
  await fg("**/*.openapi.json", {
    cwd: outputDir,
    absolute: true,
  }).then(async (swaggerFilePaths) => {
    copyIncludes(outputDir);
    for (const swaggerFilePath of swaggerFilePaths) {
      const key = path.basename(swaggerFilePath, ".openapi.json");
      keys.push(key);
      await generateNodeSwaggerClient(swaggerFilePath, true);
      console.log("Generated:", key, "client");
    }
  });
  generateIndex(keys, outputDir);
  console.log("Generated index");
};

export const findFile = (
  filePath: string,
  directory = __dirname,
  count = 0,
): string => {
  const parentDir = path.join(directory, "..");
  const current = path.join(directory, filePath);
  if (fs.existsSync(current)) {
    return current;
  }
  if (parentDir === directory) {
    throw new Error(`Could not find file ${filePath}`);
  }
  return findFile(filePath, parentDir, count + 1);
};

export async function autogenNodeClient(options: {
  swaggerJsonPath: string;
  packageName: string;
  skipPaths: string[];
  outputDir?: string;
}) {
  // get path to package.json of the calling project
  const packageJsonPath = findFile("package.json", process.cwd());
  const packageJsonDir = path.dirname(packageJsonPath);
  console.log(options?.outputDir);
  const clientDir = options?.outputDir
    ? path.resolve(options.outputDir)
    : path.join(packageJsonDir, "./node_modules", options.packageName);
  const clientSrcDir = path.join(clientDir, "src");

  console.log("Generating client in", {
    clientDir,
  });

  if (fs.existsSync(clientDir)) {
    await fs.promises.rm(clientDir, { recursive: true, force: true });
  }
  fs.mkdirSync(clientSrcDir, { recursive: true });

  let targetPath = path.join(clientSrcDir, "swagger.json");
  targetPath = path.resolve(targetPath);
  console.log(`Copying ${options.swaggerJsonPath} to ${targetPath}`);
  fs.copyFileSync(options.swaggerJsonPath, targetPath);
  await generateNodeSwaggerClient(targetPath);

  fs.writeFileSync(
    path.join(clientDir, ".npmrc"),
    `//registry.npmjs.org/:_authToken=$\{NPM_ACCESS_TOKEN}`,
  );
  fs.writeFileSync(
    path.join(clientDir, "package.json"),
    `{
        "name": "${process.env.NPM_PROJECT_TITLE || options.packageName}",
        "version": "${process.env.NEXT_PROJECT_VERSION || "0.0.0"}",
        "description": "An http client generated from openapi schema",
        "main": "./dist/index.js",
        "module": "./dist/index.mjs",
        "typings": "./dist/index.d.ts",
        "exports": {
          ".": {
            "types": "./dist/index.d.ts",
            "import": "./dist/index.mjs",
            "require": "./dist/index.js"
          },
          "./includes": {
            "types": "./dist/includes/index.d.ts",
            "default": "./dist/includes/index.js"
          },
          "./*": "./dist/*",
          "./src/*": "./src/*"
        },
        "typesVersions": {
          "*": {
            "index": [
              "dist/index.d.ts"
            ],
            "includes": [
              "dist/includes/index.d.ts"
            ]
          }
        },
        "files": ["dist", "src", "*.ts", "*.json", "README.md", "README.full.md"],
        "scripts": {
            "transpile": "tsc",
            "build": "tsup src/index.ts --format esm"
        },
        "dependencies": {
          "@zodios/core": "10.9.6",
          "axios": "1.11.0",
          "change-case": "^4.1.2",
          "zod": "3.23.8"
        },
        "devDependencies": {
          "@types/node": "20.8.7",
          "cpx": "1.5.0",
          "typescript": "5.2.2",
          "tsup": "7.2.0"
        },
        "keywords": ["Chik", "${options.packageName}", "openapi", "swagger", "client"],
        "author": "Chik",
        "license": "Chik"
    }`,
    "utf8",
  );

  fs.writeFileSync(
    path.join(clientDir, "tsconfig.json"),
    `{
      "compilerOptions": {
        "target": "es2020",
        "module": "commonjs",
        "lib": ["es2020", "DOM"],
        "declaration": true,
        "esModuleInterop": true,
        "skipLibCheck": true,
        "skipDefaultLibCheck": true,
        "forceConsistentCasingInFileNames": true,
        "allowJs": true,
        "outDir": "dist",
        "baseUrl": ".",
        "allowImportingTsExtensions": true,
        "emitDeclarationOnly": true,
        "paths": {
          "@zodios/core": ["../node_modules/@zodios/core/dist/index.d.ts"]
        }
      },
      "include": ["src"]
    }`,
    "utf8",
  );

  console.log("Installing dependencies...");
  execSync(`pnpm install`, {
    cwd: clientDir,
    stdio: "inherit",
  });

  console.log("Building...");
  try {
    execSync(`npm run transpile`, {
      cwd: clientDir,
      stdio: "inherit",
    });
  } catch (error) {
    console.warn(error);
  }
  try {
    execSync(`npm run build`, {
      cwd: clientDir,
      stdio: "inherit",
    });
  } catch (error) {
    console.warn(error);
  }

  await fg("src/includes/**/*.d.ts", {
    cwd: clientDir,
    absolute: true,
  }).then((files) => {
    for (const filePath of files) {
      fs.copyFileSync(
        filePath,
        path.join(
          clientDir,
          "dist",
          path.relative(path.join(clientDir, "src"), filePath),
        ),
      );
    }
  });

  console.log("Done!");
}

const isRunningInNode = require.main === module;
const isRunningInBun = typeof Bun !== "undefined";
if (isRunningInNode || isRunningInBun) {
  const cli = new CAC("autogen");
  function assert<TCondition>(
    condition: TCondition,
    message: string,
  ): asserts condition {
    if (!condition) {
      console.error(message);
      console.log();
      cli.outputHelp();
      process.exit(1);
    }
  }
  (async () => {
    cli
      .command("[swaggerJsonPath]")
      .option(
        "-o, --output-dir, <output directory>",
        "/path/to/output/directory",
      )
      .option(
        "-p, --package <name>",
        "Package name e.g. @mykeels/chik-client",
      )
      .option(
        "-w, --watch [true]",
        "Watch swagger file for changes and regenerate client",
      )
      .option(
        "-s, --skip-paths <path1,path2,...,pathN>",
        "Comma separated list of route paths to skip when generating the API client",
      )
      .action(
        async (
          swaggerJsonPath: string,
          options: {
            outputDir: string;
            package: string;
            skipPaths: string;
            watch?: true;
          },
        ) => {
          assert(swaggerJsonPath, "Missing [swaggerJsonPath] argument");
          assert(
            fs.existsSync(swaggerJsonPath),
            `Swagger file not found: ${swaggerJsonPath}`,
          );
          const skipPaths = (options.skipPaths || "")
            .split(",")
            .map((p) => p.trim())
            .filter(Boolean);
          const packageName = options.package || "@mykeels/api";

          const run = async () => {
            await autogenNodeClient({
              swaggerJsonPath,
              packageName,
              skipPaths,
              outputDir: options.outputDir,
            });
            if (options.watch) {
              console.log("Watching for changes in", swaggerJsonPath, "...");
            }
          };

          await run();

          if (options.watch) {
            fs.watchFile(swaggerJsonPath, async () => {
              console.log("Changes detected in", swaggerJsonPath);
              await run();
            });
          }
        },
      );
    cli.help();
    cli.parse();
  })();
}
