TARGETS		:= test

test_FILES := \
	BadAss.cs		\
	Courage.cs		\
	GenePair.cs		\
	Probability.cs	\
	Random.cs		\
	Stupidity.cs	\
	Trait.cs		\
	test.cs			\
	$e

RESGEN2		:= resgen2
GMCS		:= gmcs
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
