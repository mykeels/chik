/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */

import fs from "fs";
import path from "path";
import { CAC } from "cac";
import { execSync } from "child_process";
import { OpenAPI } from "openapi-types";
import { inc, maxSatisfying } from "semver";
import * as OpenAPIParser from "@readme/openapi-parser";

export const getCircularReplacer = () => {
  const seen = new WeakSet();
  return (_: any, value: any) => {
    if (typeof value === "object" && value !== null) {
      if (seen.has(value)) {
        return;
      }

      seen.add(value);
    }
    return value;
  };
};

const omit = <T, K extends keyof T>(obj: T, keys: K[]): Omit<T, K> => {
  return Object.fromEntries(
    Object.entries(obj as any).filter(([key]) => !keys.includes(key as K)),
  ) as Omit<T, K>;
};

export const detectAnyChanges = (
  currentSwagger: OpenAPI.Document,
  previousSwagger: OpenAPI.Document,
) => {
  const currentSwaggerJson = JSON.stringify(
    omit(currentSwagger, ["info"]),
    getCircularReplacer(),
  );
  const previousSwaggerJson = JSON.stringify(
    omit(previousSwagger, ["info"]),
    getCircularReplacer(),
  );
  return currentSwaggerJson !== previousSwaggerJson;
};

const cleanupSwagger = async (swagger: OpenAPI.Document) => {
  if (swagger.paths) {
    for (const path in swagger.paths) {
      for (const key in swagger.paths?.[path]) {
        const method = swagger.paths?.[path]?.[key];
        if (method.requestBody?.content) {
          delete method.requestBody.content["application/*+json"];
          delete method.requestBody.content["text/json"];
        }
      }
    }
  }
  return await OpenAPIParser.dereference(swagger);
};

const getCurrentSwaggerFromFS = async (swaggerFilePath: string) => {
  return await cleanupSwagger(
    JSON.parse(fs.readFileSync(swaggerFilePath, "utf-8")),
  );
};

const getPreviousSwaggerFromGit = async (
  swaggerFilePath: string,
  baseBranchName: string,
): Promise<OpenAPI.Document | null> => {
  const gitFriendlySwaggerFilePath = path
    .relative(process.cwd(), swaggerFilePath)
    .replace(/\\/g, "/");
  const sanitizedBaseBranchName = baseBranchName.replace(/^(origin\/)/, "");
  execSync(`git fetch origin ${sanitizedBaseBranchName}`);
  const gitCommand = `git show origin/${sanitizedBaseBranchName}:./${gitFriendlySwaggerFilePath}`;
  const previousSwagger = execSync(gitCommand, {
    encoding: "utf-8",
    stdio: ["pipe", "pipe", "ignore"],
    maxBuffer: 1024 * 1024 * 64,
  }).toString();
  if (!previousSwagger || previousSwagger.includes("fatal: path")) {
    console.error(
      `Could not find previous swagger file at ${gitCommand}`,
      previousSwagger,
    );
    return null;
  }
  return await cleanupSwagger(JSON.parse(previousSwagger));
};

const updateSwaggerFileVersionInFS = (
  swaggerFilePath: string,
  version: string,
) => {
  const currentSwagger = JSON.parse(fs.readFileSync(swaggerFilePath, "utf-8"));
  console.log(`Updating version in ${swaggerFilePath} to ${version}`);
  fs.writeFileSync(
    swaggerFilePath,
    JSON.stringify(
      {
        ...currentSwagger,
        info: {
          ...currentSwagger.info,
          version,
        },
      },
      null,
      2,
    ),
  );
};

const updateCsprojFileVersionInFS = (
  csprojFilePath: string,
  version: string,
) => {
  console.log(`Updating version in ${csprojFilePath} to ${version}`);
  fs.writeFileSync(
    csprojFilePath,
    fs
      .readFileSync(csprojFilePath, "utf-8")
      .replace(/<Version>(.*)<\/Version>/, `<Version>${version}</Version>`),
  );
};

const getCsProjFileVersionFromFS = (csprojFilePath: string) => {
  const csprojFile = fs.readFileSync(csprojFilePath, "utf-8");
  const match = csprojFile.match(/<Version>(.*)<\/Version>/);
  console.log(`CSProj file version: ${match ? match[1] : null}`);
  return match ? match[1] : null;
};

const getPreviousCsProjFileVersionFromGit = (
  csprojFilePath: string,
  baseBranchName: string,
) => {
  const gitFriendlyCsprojFilePath = path
    .relative(process.cwd(), csprojFilePath)
    .replace(/\\/g, "/");
  const sanitizedBaseBranchName = baseBranchName.replace(/^(origin\/)/, "");
  execSync(`git fetch origin ${sanitizedBaseBranchName}`);
  const gitCommand = `git show origin/${sanitizedBaseBranchName}:./${gitFriendlyCsprojFilePath}`;
  const previousCsproj = execSync(gitCommand, {
    encoding: "utf-8",
    stdio: ["pipe", "pipe", "ignore"],
    maxBuffer: 1024 * 1024 * 64,
  }).toString();
  if (!previousCsproj || previousCsproj.includes("fatal: path")) {
    console.error(
      `Could not find previous csproj file at ${gitCommand}`,
      previousCsproj,
    );
    return null;
  }
  const match = previousCsproj.match(/<Version>(.*)<\/Version>/);
  return match ? match[1] : null;
};

const detectBreakingChanges = async (
  title: string,
  options: {
    getImpactedProjects: () => Promise<string[]>;
    getBeforeSwagger: () => Promise<OpenAPI.Document>;
    getAfterSwagger: () => Promise<OpenAPI.Document>;
  },
) => {
  return false;
};

export const updateSwaggerVersion = async (
  swaggerFilePath: string,
  options: { csprojFilePath: string; baseBranchName: string },
  {
    getCurrentSwagger = getCurrentSwaggerFromFS,
    getPreviousSwagger = getPreviousSwaggerFromGit,
    updateSwaggerFileVersion = updateSwaggerFileVersionInFS,
    updateCsprojFileVersion = updateCsprojFileVersionInFS,
    getCsprojFileVersion = getCsProjFileVersionFromFS,
    getPreviousCsProjFileVersion = getPreviousCsProjFileVersionFromGit,
  } = {},
) => {
  const csprojFilePath = path.resolve(options.csprojFilePath);
  const baseBranchName = options.baseBranchName;

  const currentSwagger = await getCurrentSwagger(swaggerFilePath);
  const previousSwagger = await getPreviousSwagger(
    swaggerFilePath,
    baseBranchName,
  );

  const currentCsprojVersion = getCsprojFileVersion(csprojFilePath);
  const currentSwaggerVersion = currentSwagger.info.version;
  const currentVersion =
    maxSatisfying(
      currentCsprojVersion
        ? [currentCsprojVersion, currentSwaggerVersion]
        : [currentSwaggerVersion],
      "*",
    ) || currentSwaggerVersion;
  const currentSwaggerTitle = currentSwagger.info.title ?? "Under4Games.Demo";
  console.log(`Current version: ${currentVersion}`);

  let nextVersion: string | null = inc(currentVersion, "patch");
  if (!nextVersion) {
    console.error(
      `Could not determine next version from current version ${currentSwaggerVersion}`,
    );
    return;
  }
  let anyChangeDetected = false;
  if (previousSwagger) {
    anyChangeDetected = detectAnyChanges(currentSwagger, previousSwagger);
    if (!anyChangeDetected) {
      nextVersion = currentVersion;
      console.log(`No changes detected, skipping update`);
    } else {
      const previousCsprojVersion = getPreviousCsProjFileVersion(
        csprojFilePath,
        baseBranchName,
      );
      const previousSwaggerVersion = previousSwagger.info.version;
      const previousVersion =
        maxSatisfying(
          previousCsprojVersion
            ? [previousCsprojVersion, previousSwaggerVersion]
            : [previousSwaggerVersion],
          "*",
        ) || previousSwaggerVersion;
      console.log(`Previous version: ${previousVersion}`);
      try {
        const breakingChanges = await detectBreakingChanges(
          currentSwaggerTitle,
          {
            getImpactedProjects: async () => [],
            getBeforeSwagger: async () => previousSwagger,
            getAfterSwagger: async () => currentSwagger,
          },
        );
        if (breakingChanges) {
          console.warn(breakingChanges);
        }
        nextVersion = inc(previousVersion, breakingChanges ? "major" : "patch");
      } catch (e: unknown) {
        console.error(`Error detecting breaking changes`, e);
        nextVersion = inc(previousVersion, "patch");
      }
    }
  }

  updateSwaggerFileVersion(swaggerFilePath, nextVersion!);
  if (csprojFilePath && fs.existsSync(csprojFilePath)) {
    updateCsprojFileVersion(csprojFilePath, nextVersion!);
  }
  if (anyChangeDetected) {
    console.log(`Changes detected, updating client`);
    execSync(`npm run autogen`);
  }
  return nextVersion;
};

const isRunningInNode = require.main === module;
const isRunningInBun = typeof Bun !== "undefined";
if (isRunningInNode || isRunningInBun) {
  const cli = new CAC("update-swagger-version");
  (async () => {
    const assertArgument = (arg: string, message: string) => {
      if (!arg) {
        console.error(message);
        cli.outputHelp();
        process.exit(1);
      }
    };

    cli
      .command("[swaggerFilePath]")
      .option(
        "-b, --base-branch-name <baseBranchName>",
        "Name of the base branch to compare agaist e.g. master or development",
        { default: "origin/main" },
      )
      .option(
        "-p, --csproj-file-path <csprojFilePath>",
        "/path/to/[Project].csproj",
      )
      .action(
        async (
          swaggerFilePath: string,
          options: {
            csprojFilePath: string;
            baseBranchName: string;
          },
        ) => {
          assertArgument(swaggerFilePath, "Missing [swaggerFilePath] argument");
          assertArgument(
            options.csprojFilePath,
            "Missing --csproj-file-path argument",
          );
          assertArgument(
            options.baseBranchName,
            "Missing --base-branch argument",
          );
          return updateSwaggerVersion(path.resolve(swaggerFilePath), options);
        },
      );
    cli.help();
    cli.parse();
  })();
}
