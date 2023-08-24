@ECHO OFF
CLS

rmdir ..\documentation /s /q

xmldocmd ..\src\bin\Debug\netstandard2.0\iTin.Mail.Smtp.Net.dll ..\documentation

PAUSE
