using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TRexLib
{
    public class TestResultSet : IEnumerable<TestResult>
    {
        private readonly Lazy<(List<TestResult> all, List<TestResult> passed, List<TestResult> failed, List<TestResult> notExecuted)> evaluated;

        public TestResultSet(IEnumerable<TestResult> testResults)
        {
            evaluated = new Lazy<(List<TestResult>, List<TestResult>, List<TestResult>, List<TestResult>)>(() =>
            {
                var all = new List<TestResult>();
                var passed = new List<TestResult>();
                var failed = new List<TestResult>();
                var notExecuted = new List<TestResult>();

                foreach (var testResult in testResults.OrderBy(r => r.FullyQualifiedTestName))
                {
                    all.Add(testResult);

                    switch (testResult.Outcome)
                    {
                        case TestOutcome.Passed:
                            passed.Add(testResult);
                            break;
                        case TestOutcome.Failed:
                            failed.Add(testResult);
                            break;
                        case TestOutcome.NotExecuted:
                            notExecuted.Add(testResult);
                            break;
                    }
                }

                return (all, passed, failed, notExecuted);
            });
        }

        public IReadOnlyCollection<TestResult> Passed => evaluated.Value.passed;

        public IReadOnlyCollection<TestResult> Failed => evaluated.Value.failed;

        public IReadOnlyCollection<TestResult> NotExecuted => evaluated.Value.notExecuted;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TestResult> GetEnumerator() => evaluated.Value.all.GetEnumerator();
    }
}
