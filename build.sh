#!/usr/bin/env bash

build() {
    xbuild rethinkdb-net.sln

    if [[ $? != 0 ]] ; then
        build_failed
    else
        build_succeeded
    fi
}

build_failed() {
    print_status "BUILD FAILED"
}

build_succeeded() {
    print_status "BUILD SUCCEEDED"
    run_tests
}

run_tests() {
    RUNNER_PATH="packages/NUnit.Runners.2.6.1/tools"
    mono ${RUNNER_PATH}/nunit-console.exe rethinkdb-net-test/bin/Debug/rethinkdb-net-test.dll
}

print_status() {
    echo ""
    echo "*** $1 ***"
    echo ""
}

build