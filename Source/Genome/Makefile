TARGETS		:= test

test_FILES := \
	../Utils/Probability.cs	\
	../Utils/Random.cs	\
	BadAss.cs		\
	Courage.cs		\
	Gender.cs		\
	GenePair.cs		\
	Stupidity.cs	\
	Trait.cs		\
	test.cs			\
	$e

RESGEN2		:= resgen2
GMCS		:= mcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: ${TARGETS}

test: ${test_FILES}
	${GMCS} ${GMCSFLAGS} \
		-out:$@ $^

clean:
	rm -f ${TARGETS}

.PHONY: all clean
