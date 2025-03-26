import { readFileSync } from "node:fs";
import { exit } from "node:process";

export type Repo = {
  name: string;
  isExternal: boolean;
  forceExternal: boolean | undefined;
  externalVersion: string | undefined;
  branchNameExcludes: string | undefined;
  dirName: string;
  protoDirs: ProtoDir[];
};
export type ProtoDir = {
  in: string;
  out: string;
};

export type ProtoChain = {
  repoDir: string;
  chainRepoName: string | null;
  chainRepoPath: string;
  commitHash: string;
  protoDir: string;
  hasNoProtos: boolean | undefined;
  goModPath: string | undefined;

  protoDependencies: Repo[];
};

export function loadProtoChainFromFile(path: string) {
  try {
    const content = readFileSync(path, "utf8").trim();
    return JSON.parse(content) as ProtoChain;
  } catch (error) {
    console.error(`Failed reading config at ${path}`);
    exit(1);
  }
}
