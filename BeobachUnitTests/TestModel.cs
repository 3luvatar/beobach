using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace BeobachUnitTests
{
    public class TestModel
    {
        public readonly ObservableProperty<string> Name = new ObservableProperty<string>();
        public readonly ObservableProperty<TestEnum> TestEnumValue = new ObservableProperty<TestEnum>();

        public TestModel(string name)
        {
            Name.Value = name;
        }
    }

    public enum TestEnum
    {
        one,
        two,
        three
    }
}
