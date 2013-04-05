#!/usr/bin/env bash

build_failed() {
    echo ""
    echo "*** BUILD FAILED ***"
    echo ""
}

build_succeeded() {
    echo ""
    echo "*** BUILD SUCCEEDED ***"
    echo ""
    echo "$(run_tests)"
}

run_tests() {
    RUNNER_PATH="packages/NUnit.Runners.2.6.1/tools"
    mono ${RUNNER_PATH}/nunit-console.exe rethinkdb-net-test/bin/Debug/rethinkdb-net-test.dll
}

xbuild rethinkdb-net.sln

EXIT_CODE=$?

if [[ $EXIT_CODE != 0 ]] ; then
    echo "$(build_failed)"
else
    echo "$(build_succeeded)"
fi