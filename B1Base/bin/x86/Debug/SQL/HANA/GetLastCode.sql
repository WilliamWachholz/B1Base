select case when coalesce((select "U_NextCode" from "@{1}" where "U_UserTable" = '@{0}'), 1) > coalesce((select top 1 "U_Code" + 1 from "@{0}" order by "U_Code" desc), 1)
then coalesce((select "U_NextCode" from "@{1}" where "U_UserTable" = '@{0}'), 1)  else coalesce((select top 1 "U_Code" + 1 from "@{0}" order by "U_Code" desc), 1) end
from dummy