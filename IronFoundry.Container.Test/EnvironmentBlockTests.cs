﻿namespace IronFoundry.Warden.Test.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using IronFoundry.Warden.Utilities;
    using Xunit;

    public class EnvironmentBlockTests
    {
        [Fact]
        public void CanBeBuiltFromDictionary()
        {
            var env = EnvironmentBlock.Create(new Dictionary<string, string> {{"FOO", "BAR"}});
            var dict = env.ToDictionary();
            Assert.Equal(1, dict.Count);
            Assert.Equal("BAR", dict["FOO"]);
        }

        [Fact]
        public void GeneratesDefaultEnvironment()
        {
            var env = EnvironmentBlock.GenerateDefault();
            var dict = env.ToDictionary();

            Assert.True(dict.Count > 0);
            // Verify some of the environment variables we expect to be there by default
            Assert.Contains("TEMP", dict.Keys);
            Assert.Contains("TMP", dict.Keys);
            Assert.Contains("SystemRoot", dict.Keys);
            Assert.Contains("COMPUTERNAME", dict.Keys);
        }

        [Fact]
        public void MergeOverwritesOld()
        {
            var env = EnvironmentBlock.GenerateDefault();
            var newEnv = EnvironmentBlock.Create(new Hashtable {{"COMPUTERNAME", "FOOBAR"}});
            var dict = env.Merge(newEnv).ToDictionary();

            Assert.Equal(env.ToDictionary().Count, dict.Count);
            Assert.Equal("FOOBAR", dict["COMPUTERNAME"]);
        }
    }
}