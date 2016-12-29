#! /bin/sh

full_version=`./tools/git-version-gen --prefix v .tarball-version`
version=`echo $full_version | sed -e 's/-/\t/' | cut -f 1 | sed -e 's/UNKNOWN/0.0.0/'` 

sed -e "s/@FULL_VERSION@/$full_version/" -e "s/@VERSION@/$version/" assembly/AssemblyInfo.in > assembly/AssemblyInfo.cs-
cmp -s assembly/AssemblyInfo.cs assembly/AssemblyInfo.cs- || mv assembly/AssemblyInfo.cs- assembly/AssemblyInfo.cs
rm -f assembly/*.cs-

sed -e "s/@FULL_VERSION@/$full_version/" -e "s/@VERSION@/$version/" KerbalStats.dox.in > KerbalStats.dox-
cmp -s KerbalStats.dox KerbalStats.dox- || mv KerbalStats.dox- KerbalStats.dox
rm -f KerbalStats.dox-

