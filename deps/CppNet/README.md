CppNet
======

Quick and dirty port of JCPP (https://github.com/shevek/jcpp) to C#, with features to support Clang preprocessing.

Features added from jcpp:
* __has_include, __has_include_next, __has_feature
* variadic macros
* #import


```C#
var pp = new Preprocessor();
pp.addFeature(Feature.DIGRAPHS);
pp.addFeature(Feature.TRIGRAPHS);
pp.addFeature(Feature.OBJCSYNTAX);
pp.addWarning(Warning.IMPORT);
pp.addFeature(Feature.INCLUDENEXT);
pp.setListener(new PreprocessorListener());

pp.getSystemIncludePath().Add(@"C:\XcodeDefault.xctoolchain\usr\include");
pp.getSystemIncludePath().Add(@"C:\XcodeDefault.xctoolchain\usr\lib\clang\6.0\include");

pp.addMacro("__AARCH64_SIMD__");
pp.addMacro("__ARM64_ARCH_8__");
pp.addMacro("__ARM_NEON__");
pp.addMacro("__LITTLE_ENDIAN__");
pp.addMacro("__REGISTER_PREFIX__", "");
pp.addMacro("__arm64", "1");
pp.addMacro("__arm64__", "1");
pp.addMacro("__GNUC__", "4");
pp.addMacro("OBJC_NEW_PROPERTIES");
pp.addMacro("__STDC_HOSTED__", "1");
pp.addMacro("__MACH__");
Version version = new Version("8.0.0.0");
pp.addMacro("__STATIC__");

pp.addInput(new FileLexerSource("test.m"));
```
