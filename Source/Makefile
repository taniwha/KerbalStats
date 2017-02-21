KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KSGAMEDATA  := ${GAMEDATA}/KerbalStats
PLUGINDIR	:= ${KSGAMEDATA}/Plugins

TARGETS		:= bin/KerbalStats.dll bin/Progeny.dll
DATA		:= \
	KerbalStatsWrapper.cs		\
	Experience/seat_tasks.cfg	\
	$e

KS_CORE := \
	Utils/EnumUtil.cs				\
	Utils/MathUtil.cs				\
	Utils/ModuleLoader.cs			\
	IKerbalExt.cs					\
	KerbalExt.cs					\
	KerbalStats.cs					\
	KerbalStatsScenario.cs			\
	assembly/AssemblyInfo.cs		\
	assembly/VersionReport.cs		\
	$e

KS_EXPERIENCE := \
	Experience/Body.cs				\
	Experience/Events.cs			\
	Experience/Experience.cs		\
	Experience/PartSeatTasks.cs		\
	Experience/SeatTasks.cs			\
	Experience/Task.cs				\
	Experience/Tracker.cs			\
	$e

KS_GENOME := \
	Genome/BadAss.cs				\
	Genome/Courage.cs				\
	Genome/Gender.cs				\
	Genome/GenePair.cs				\
	Genome/Genome.cs				\
	Genome/Probability.cs			\
	Genome/Stupidity.cs				\
	Genome/Trait.cs					\
	$e

KS_PROGENY := \
	Progeny/Location/AstronautComplex.cs	\
	Progeny/Location/EVA.cs					\
	Progeny/Location/Location.cs			\
	Progeny/Location/LocationTracker.cs		\
	Progeny/Location/Tomb.cs				\
	Progeny/Location/Unknown.cs				\
	Progeny/Location/VesselPart.cs			\
	Progeny/Location/Wilds.cs				\
	Progeny/Location/Womb.cs				\
	Progeny/Traits/AgingTimeK.cs			\
	Progeny/Traits/AgingTimeP.cs			\
	Progeny/Traits/BioClockTC.cs			\
	Progeny/Traits/BioClockInverse.cs		\
	Progeny/Traits/CyclePeriodK.cs			\
	Progeny/Traits/CyclePeriodP.cs			\
	Progeny/Traits/GameteLifeK.cs			\
	Progeny/Traits/GameteLifeP.cs			\
	Progeny/Traits/GestationPeriodK.cs		\
	Progeny/Traits/GestationPeriodP.cs		\
	Progeny/Traits/InterestK.cs				\
	Progeny/Traits/InterestTC.cs			\
	Progeny/Traits/MaturationTimeK.cs		\
	Progeny/Traits/MaturationTimeP.cs		\
	Progeny/Traits/OvulationTimeK.cs		\
	Progeny/Traits/OvulationTimeP.cs		\
	Progeny/Traits/PRange.cs				\
	Progeny/Traits/TimeK.cs					\
	Progeny/Traits/TimeP.cs					\
	Progeny/Zygote/Adult.cs					\
	Progeny/Zygote/BioClock.cs				\
	Progeny/Zygote/Cycle.cs					\
	Progeny/Zygote/Embryo.cs				\
	Progeny/Zygote/Female.cs				\
	Progeny/Zygote/Gamete.cs				\
	Progeny/Zygote/Interest.cs				\
	Progeny/Zygote/Juvenile.cs				\
	Progeny/Zygote/Male.cs					\
	Progeny/Zygote/Zygote.cs				\
	Progeny/DebugWindow.cs			\
	Progeny/IKerbal.cs				\
	Progeny/Progeny.cs				\
	Progeny/Settings.cs				\
	Progeny/Tracker.cs				\
	toolbar/Toolbar.cs				\
	toolbar/ToolbarWrapper.cs		\
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

KS_FILES := ${KS_CORE} ${KS_EXPERIENCE}

PR_FILES := ${KS_GENOME} ${KS_PROGENY}

bin/KerbalStats.dll: ${KS_FILES}
	@mkdir -p bin
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-out:$@ $^

bin/Progeny.dll: ${PR_FILES} bin/KerbalStats.dll
	@mkdir -p bin
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-lib:bin -r:KerbalStats \
		-out:$@ ${PR_FILES}

clean:
	rm -f ${TARGETS} KerbalStats.dox assembly/AssemblyInfo.cs
	rm -rf docs
	test -d bin && rmdir bin || true

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
	cp ${DATA} ${KSGAMEDATA}

doc: version
	doxygen KerbalStats.dox

.PHONY: all clean install doc
