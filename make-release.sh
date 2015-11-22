#!/bin/bash

shopt -s globstar
set -e

NEW_VERSION=$1

if ! [[ $NEW_VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]] ; then
    echo "Argument must match [0-9]+.[0-9]+.[0-9]+.[0-9]+"
    exit
fi

echo "Processing release $NEW_VERSION"

# ensure all files have the same line endings
find . -type f -name '*.cs' -exec dos2unix -m '{}' \;

for f in **/AssemblyInfo.cs; do
    sed --in-place -e "s/\"[0-9]\+\.[0-9]\+\.[0-9]\+\.[0-9]\+\"/\"$NEW_VERSION\"/" $f
    if ! grep -q "$NEW_VERSION" "$f"
    then
        echo "sed failed $f"
        exit
    fi
done

sed --in-place -e "s#<version>[0-9]\+\.[0-9]\+\.[0-9]\+\.[0-9]\+</version>#<version>$NEW_VERSION</version>#" rethinkdb-net.nuspec
if ! grep -q "<version>$NEW_VERSION</version>" rethinkdb-net.nuspec
then
    echo "sed failed"
    exit
fi


sed --in-place -e "s#<version>[0-9]\+\.[0-9]\+\.[0-9]\+\.[0-9]\+</version>#<version>$NEW_VERSION</version>#" rethinkdb-net-newtonsoft.nuspec
if ! grep -q "<version>$NEW_VERSION</version>" rethinkdb-net-newtonsoft.nuspec
then
    echo "sed failed"
    exit
fi

sed --in-place -e "s#<dependency id=\"rethinkdb-net\" version=\"[0-9]\+\.[0-9]\+\.[0-9]\+\.[0-9]\+\" />#<dependency id=\"rethinkdb-net\" version=\"$NEW_VERSION\" />#" rethinkdb-net-newtonsoft.nuspec
if ! grep -q "<dependency id=\"rethinkdb-net\" version=\"$NEW_VERSION\" />" rethinkdb-net-newtonsoft.nuspec
then
    echo "sed failed"
    exit
fi

sed --in-place -e "s/## Next Release/## $NEW_VERSION (`date +%Y-%m-%d`)/" RELEASE-NOTES.md
if ! grep -q "## $NEW_VERSION (`date +%Y-%m-%d`)" RELEASE-NOTES.md
then
    echo "sed failed"
    exit
fi

echo "Files automatically changed.  Now it's up to you to diff and "
echo "double-check everything!  If any changes occurred due to"
echo "dos2unix conversion, those changes should be committed before"
echo "proceeding."
echo
echo "Things to check for manual-intervention:"
echo
echo "  * Release notes are up-to-date."
echo "  * <releaseNotes> in rethinkdb-net.nuspec don't need changes."
echo
read -p "Build, commit, upload? [yN] " -n 1 -r
echo    # (optional) move to a new line
if ! [[ $REPLY =~ ^[Yy]$ ]]
then
    echo
    echo "Aborting!"
    echo
    exit
fi

xbuild /p:Configuration=Release rethinkdb-net.sln
nuget pack rethinkdb-net.nuspec
nuget pack rethinkdb-net-newtonsoft.nuspec
git commit -a -m"Update version to $NEW_VERSION"
git push
git tag -a -m"Version ${NEW_VERSION}" v${NEW_VERSION}
git push --tags
git branch -d latest-release
git branch latest-release v${NEW_VERSION}
git push -f origin latest-release

echo
echo "Manual steps remaining:"
echo "  1. Update release information on GitHub w/ subset of release notes"
echo "     relevant to the tag (https://github.com/mfenniak/rethinkdb-net/releases)"
echo "  2. Add .nupkg files to GitHub release."
echo "  3. Upload .nupkg files to https://nuget.org/packages/upload"
echo
