select coalesce((select top 1 "U_Code" + 1 from "@{0}" order by "U_Code" desc), 
coalesce((select "U_NextCode" from "@{1}" where "U_UserTable" = '@{0}'), 1)) from dummy