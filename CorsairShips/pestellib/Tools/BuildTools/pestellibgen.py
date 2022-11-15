import os
import argparse
import uuid
import re

parser = argparse.ArgumentParser(description='Generates PestelLib solution with custom modules')
parser.add_argument('--project-root', type=str, required=True, help='project root dir')
parser.add_argument('--sln-name', type=str, required=False, default="PestelLibTest.sln", help='name of generated solution')

args = parser.parse_args()

TEMPLATE_PROJ_MAP = {'SharedLogicTemplate': 'ConcreteSharedLogic'}

PROJECT_ROOT = args.project_root
OUTPUT_SOLUTION = os.path.join(PROJECT_ROOT, args.sln_name)
NS_OUTPUT_SOLUTION = os.path.join(PROJECT_ROOT, args.sln_name[:-4] + '.standard.sln')
PESTELLIB_DIR = os.path.join(PROJECT_ROOT, "pestellib")
ADDITIONAL_PROJECTS_DIR = os.path.join(PROJECT_ROOT, "ProjectLib")
PESTEL_PROJECTS_SLINKS_DIR = os.path.join(PROJECT_ROOT, "Assets", "Plugins", "LibsSymlinks")
PROJECT_BUILD_CONFIG_TMP="\t\t%s.Debug|Any CPU.ActiveCfg = Debug|Any CPU\n\t\t%s.Debug|Any CPU.Build.0 = Debug|Any CPU\n\t\t%s.Release|Any CPU.ActiveCfg = Release|Any CPU\n\t\t%s.Release|Any CPU.Build.0 = Release|Any CPU\n"

class SolutionWriter():
    def __init__(self):
        pass
    def getGuid(self):
        return ('{' + uuid.uuid4().urn[9:] + '}').upper()
    def write(self, solutionFileName, projectsPaths):
        pass

class VS2017SolutionWriter(SolutionWriter):
    def write(self, solutionFileName, projectsPaths, slnDeps):
        guids = {}
        backendDeps = []
        for p in projectsPaths:
            name = os.path.basename(p)[:-7]
            guids[name] = self.getGuid()
            if name.startswith("Dep"):
                backendDeps.append(name)
        if slnDeps.has_key("Backend"):
            slnDeps["Backend"].extend(backendDeps)
        else:
            slnDeps["Backend"] = backendDeps
        if slnDeps.has_key("CoreBackend"):
            slnDeps["CoreBackend"].extend(backendDeps)
        else:
            slnDeps["CoreBackend"] = backendDeps
        with open(solutionFileName, "w") as f:
            solutionGUID = self.getGuid()
            f.write('Microsoft Visual Studio Solution File, Format Version 12.00\n')
            f.write('# Visual Studio 15\n')
            f.write('VisualStudioVersion = 15.0.27130.2010\n')
            f.write('MinimumVisualStudioVersion = 10.0.40219.1\n')
            solutionDir = os.path.dirname(solutionFileName)
            projectsBuildConfigs = ""
            for p in projectsPaths:
                rel = os.path.relpath(p, solutionDir)
                name = os.path.basename(p)[:-7]
                projGuid = guids[name]
                projectsBuildConfigs += PROJECT_BUILD_CONFIG_TMP % (projGuid, projGuid, projGuid, projGuid)
                depsStr = ''
                if slnDeps is not None and slnDeps.has_key(name):
                    slnProjDeps = slnDeps[name]
                    depsStr = '\tProjectSection(ProjectDependencies) = postProject\n'
                    for d in slnProjDeps:
                        if d in TEMPLATE_PROJ_MAP.keys():
                            d = TEMPLATE_PROJ_MAP[d]
                        depGuid = guids.get(d)
                        if depGuid is None:
                            print 'Dependent project %s of %s guid not found. Skipping' % (d, name)
                        else:
                            depsStr += '\t\t%s = %s\n' % (depGuid, depGuid)
                    depsStr += '\tEndProjectSection\n'

                f.write('Project("%s") = "%s", "%s", "%s"\n%sEndProject\n' % (solutionGUID, name, rel, projGuid, depsStr))
            f.write('Global\n')

            f.write('\tGlobalSection(SolutionConfigurationPlatforms) = preSolution\n')
            f.write('\t\tDebug|Any CPU = Debug|Any CPU\n')
            f.write('\t\tRelease|Any CPU = Release|Any CPU\n')
            f.write('\tEndGlobalSection\n')
            f.write('\tGlobalSection(ProjectConfigurationPlatforms) = postSolution\n')
            f.write(projectsBuildConfigs)
            f.write('\tEndGlobalSection\n')
            f.write('EndGlobal\n')

def CsProjectsLookup(path):
    result = []
    if not os.path.exists(path):
        os.makedirs(path)
        return []
    for entry in os.listdir(path):
        p = os.path.join(path, entry)
        if os.path.isdir(p):
            result.extend(CsProjectsLookup(p))
        elif entry[-6:].lower() == 'csproj':
            result.append(p)
    return result

def HasSources(path):
    for entry in os.listdir(path):
        entryPath = os.path.join(path, entry)
        if os.path.isdir(entryPath):
            hasSources = HasSources(entryPath)
            if hasSources:
                return True
        if entry[-2:].lower() == 'cs':
            return True
    return False

def SourceBasedProjectsLookup(path):
    result = []
    for proj in os.listdir(path):
        projDir = os.path.join(path, proj)
        if not os.path.isdir(projDir):
            continue
        if proj == "Editor":
            editorProjes = SourceBasedProjectsLookup(projDir)
            result.extend(editorProjes)
        else:
            hasSource = HasSources(projDir)
            if hasSource:
                result.append(proj)
    result.append("Backend")
    result.append("CoreBackend")
    return result

rgxProjectRef = re.compile('ProjectReference Include="(.*?)"', re.IGNORECASE | re.MULTILINE | re.DOTALL)
rgxNetStandardProject = re.compile('.*<TargetFramework>(netstandard|netcoreapp).*?</TargetFramework>.*', re.IGNORECASE | re.MULTILINE | re.DOTALL)

def AddProjectsReferences(projectPaths):
    result = []
    result.extend(projectPaths)
    for p in projectPaths:
        basePath = os.path.dirname(p)
        with open(p, 'r') as f:
            data = f.read()
        for ref in rgxProjectRef.findall(data):
            refPath = os.path.abspath(os.path.join(basePath, str(ref).replace('\\', '/'))).replace("PestelLib", "pestellib")
            if refPath not in result:
                result.append(refPath)
                result = AddProjectsReferences(result)
    return result

def GetNetStandardProjects(projectsPaths):
    result = []
    for pp in projectsPaths:
        with open(pp, 'r') as f:
            data = f.read()
        m = rgxNetStandardProject.match(data)
        if m is None:
            continue
        result.append(pp)
    return result

rgxSlnProjectDef = re.compile('Project.*?EndProject$', re.MULTILINE | re.DOTALL)
rgxSlnProjectDeps = re.compile('ProjectSection\(ProjectDependencies\)(.*?)EndProjectSection', re.MULTILINE | re.DOTALL)
def SolutionDeps(path):
    result = {}
    resolver = {}
    unresolved = []
    with open(path,'r') as f:
        data = f.read()
    for proj in rgxSlnProjectDef.findall(data):
        header = proj.split('\n', 1)[0]
        header_params = header.split('=')[1].split(',')
        proj_name = header_params[0].strip()[1:-1]
        proj_guid = header_params[2].strip()[1:-1]
        resolver[proj_guid] = proj_name
        deps = []
        m = rgxSlnProjectDeps.search(proj)
        if m is None:
            continue

        depStr = m.group(0)
        depsDefs = depStr.split('\n')[1:-1]
        for d in depsDefs:
            t = d.split('=')
            t[0] = t[0].strip()
            t[1] = t[1].strip()
            if t[0] != t[1]:
                print "Unexpected solution format: ProjectDependencies section of %s contains not equal deps %s. Skipping" % (proj_name, d)
                continue
            depName = resolver.get(t[0])
            if depName is None:
                unresolved.append(t[0])
                deps.append(t[0])
            else:
                deps.append(depName)
        if len(deps) == 0:
            continue
        result[proj_name] = deps

    for ur in unresolved:
        depName = resolver.get(ur)
        if depName is None:
            skippedProjects = []
            for k in result.keys():
                if ur in result[k]:
                    skippedProjects.append(k)
                    del result[k]
            print "Unresolved project guid %s, skipping projects (%s) with this dep" % (ur, ','.join(skippedProjects))
        else:
            for k in result.keys():
                for i,v in enumerate(result[k]):
                    if v == ur:
                        result[k][i] = depName

    return result


pestelLibSlnDeps = SolutionDeps(os.path.join(PESTELLIB_DIR, 'PestelLib.sln'))

add_projects = CsProjectsLookup(ADDITIONAL_PROJECTS_DIR)
pestel_projects = CsProjectsLookup(PESTELLIB_DIR)
shared_projects_names = SourceBasedProjectsLookup(PESTEL_PROJECTS_SLINKS_DIR)

for p in pestel_projects:
    projName = os.path.basename(p)[:-7]
    if projName in shared_projects_names:
        add_projects.append(p)

add_projects = list(set(AddProjectsReferences(add_projects)))
netStandardProjects = GetNetStandardProjects(add_projects)

solutionWriter = VS2017SolutionWriter()
solutionWriter.write(OUTPUT_SOLUTION, add_projects, pestelLibSlnDeps)
if len(netStandardProjects) > 0:
    solutionWriter.write(NS_OUTPUT_SOLUTION, netStandardProjects, pestelLibSlnDeps)

pass