import { readFileSync } from "node:fs";

export type Repo = {
  name: string;
  isExternal: boolean;
  forceExternal: boolean | undefined;
  externalVersion: string | undefined;
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
  const content = readFileSync(path, "utf8").trim();
  return JSON.parse(content) as ProtoChain;
}
