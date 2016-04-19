KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KSGAMEDATA  := ${GAMEDATA}/KerbalStats
PLUGINDIR	:= ${KSGAMEDATA}/Plugins

TARGETS		:= bin/KerbalStats.dll
DATA		:= \
	KerbalStatsWrapper.cs		\
	License.txt					\
	README.md					\
	Experience/seat_tasks.cfg	\
	$e

KS_FILES := \
	Experience/Body.cs				\
	Experience/Events.cs			\
	Experience/Experience.cs		\
	Experience/PartSeatTasks.cs		\
	Experience/SeatTasks.cs			\
	Experience/Task.cs				\
	Experience/Tracker.cs			\
	IKerbalExt.cs					\
	KerbalExt.cs					\
	KerbalStats.cs					\
	assembly/AssemblyInfo.cs		\
	assembly/VersionReport.cs		\
	$e

RESGEN2		:= resgen2
GMCS		:= mcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS}

.PHONY: version
version:
	@./tools/git-version.sh

info:
	@echo "KerbalStats Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

bin/KerbalStats.dll: ${KS_FILES}
	@mkdir -p bin
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-r:KSPUtil \
		-out:$@ $^

clean:
	rm -f ${TARGETS} assembly/AssemblyInfo.cs
	test -d bin && rmdir bin || true

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
	cp ${DATA} ${KSGAMEDATA}

.PHONY: all clean install
