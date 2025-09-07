podman run --rm ^
  -v %cd%\grammar:/input:Z ^
  -v %cd%\generated:/output:Z ^
  antlr4-official ^
  sh -c "ls /input && ls /output && antlr4 -Dlanguage=CSharp -visitor -Xlog -o /output /input/OData.g4"
