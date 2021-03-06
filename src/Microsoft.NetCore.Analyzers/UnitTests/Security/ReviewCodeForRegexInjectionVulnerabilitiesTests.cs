﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForRegexInjectionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForRegexInjectionVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForRegexInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForRegexInjectionVulnerabilities();
        }

        [Fact]
        public void Constructor_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Text.RegularExpressions;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        new Regex(input);
    }
}",
                GetCSharpResultAt(11, 9, 10, 24, "Regex.Regex(string pattern)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void Constructor_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Text.RegularExpressions;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        new Regex(""^\\d{1,10}$"");
    }
}");
        }

        [Fact]
        public void IsMatch_Static_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Text.RegularExpressions;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Regex.IsMatch(""string to check"", input);
    }
}",
                GetCSharpResultAt(11, 9, 10, 24, "bool Regex.IsMatch(string input, string pattern)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void IsMatch_Static_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Text.RegularExpressions;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Regex.IsMatch(input, ""^[a-z]{1,128}$"");
    }
}");
        }

        [Fact]
        public void IsMatch_Instance_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Text.RegularExpressions;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Regex r = new Regex(""^[a-z]{1,128}$"");
        r.IsMatch(input);
    }
}");
        }
    }
}
