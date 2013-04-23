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
    return 1
}

build_succeeded() {
    print_status "BUILD SUCCEEDED"
    run_tests
}

run_tests() {
    RUNNER_PATH="packages/NUnit.Runners.2.6.1/tools"
    mono ${RUNNER_PATH}/nunit-console.exe rethinkdb-net-test/bin/Debug/rethinkdb-net-test.dll
    
    if [[ $? != 0 ]] ; then
        tests_failed
    else
        tests_passed
    fi
}

tests_failed() {
    print_status "TESTS FAILED"
    return 2
}

tests_passed() {
    print_status "TESTS PASSED"
}

print_status() {
    echo ""
    echo "*** $1 ***"
    echo ""
}

build

EXIT_CODE=$?
echo "Exit [$EXIT_CODE]"
exit $EXIT_CODE