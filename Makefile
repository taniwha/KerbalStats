KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KSGAMEDATA  := ${GAMEDATA}/KerbalStats
PLUGINDIR	:= ${KSGAMEDATA}/Plugins

TARGETS		:= KerbalStats.dll
DATA		:= License.txt seat_tasks.cfg

KS_FILES := \
    AssemblyInfo.cs					\
	Experience/Experience.cs		\
	Experience/PartSeatTasks.cs		\
	Experience/SeatTasks.cs			\
	Experience/Task.cs				\
	Experience/Tracker.cs			\
	Gender.cs						\
	KerbalStats.cs					\
	VersionReport.cs				\
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

clean:
	rm -f ${TARGETS} AssemblyInfo.cs

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
	cp ${DATA} ${KSGAMEDATA}

.PHONY: all clean install
