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
    print_status "RUNNING TESTS"
    
    start_rethink
    
    RUNNER_PATH="packages/NUnit.Runners.2.6.1/tools"
    mono ${RUNNER_PATH}/nunit-console.exe rethinkdb-net-test/bin/Debug/rethinkdb-net-test.dll
    
    local test_result=$?
    
    stop_rethink
    
    if [[ "${test_result}" != 0 ]] ; then
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

start_rethink() {
    print_status "STARTING RETHINKDB"
    remove_rethink_data_directory
    rethinkdb create
    rethinkdb serve &
}

stop_rethink() {
    print_status "STOPPING RETHINKDB"
    kill $!
    remove_rethink_data_directory
}

remove_rethink_data_directory() {
    local rethink_data_dir="rethinkdb_data"
    if [ -d "${rethink_data_dir}" ]; then
        rm -rf "${rethink_data_dir}"
    fi
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