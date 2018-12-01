select top(1) isnull(U_Code, 0) + 1
from [@{0}]
order by U_Code desc