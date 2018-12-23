select coalesce(
(select top 1 "U_Code"
from "@{0}"
order by "U_Code" desc), 0) + 1 from dummy