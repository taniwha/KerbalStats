KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KSGAMEDATA  := ${GAMEDATA}/KerbalStats
PLUGINDIR	:= ${KSGAMEDATA}/Plugins
TBGAMEDATA  := ${GAMEDATA}/000_Toolbar

TARGETS		:= KerbalStats.dll KerbalStatsToolbar.dll

KS_FILES := \
    AssemblyInfo.cs	\
	Experience.cs \
	KerbalStats.cs \
	VersionReport.cs \
	$e

KSTB_FILES := \
	AssemblyInfoToolbar.cs	\
	Toolbar.cs				\
	$e

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS}

.PHONY: version
version:
	@./git-version.sh

info:
	@echo "KerbalStats Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

KerbalStats.dll: ${KS_FILES}
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:$@ $^

KerbalStatsToolbar.dll: ${KSTB_FILES} KerbalStats.dll
	${GMCS} ${GMCSFLAGS} -t:library -lib:${TBGAMEDATA},${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-r:Toolbar \
		-r:KerbalStats \
		-out:$@ ${KSTB_FILES}

clean:
	rm -f ${TARGETS} AssemblyInfoToolbar.cs AssemblyInfo.cs

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}

.PHONY: all clean install
