import csharp
import semmle.code.csharp.metrics.Complexity

from Method m
where
  m.getCyclomaticComplexity() > 10
select m, m.getCyclomaticComplexity(), m.getLocation()