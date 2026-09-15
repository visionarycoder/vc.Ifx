

// Enable parallel test execution to satisfy MSTEST0001 analyzer warning.
// Workers = 0 lets the test framework choose an appropriate number of threads.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
