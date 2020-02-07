select case coalesce(case '{0}' when '2048' then (select count(*) from CPRF where "UserSign" = {1} and "FormID" = '169' and "ItemID" in ('2050', '2049', '2053') and "VisInForm" = 'N')
				   when '2304' then (select count(*) from CPRF where "UserSign" = {1} and "FormID" = '169' and "ItemID" in ('39698', '2305', '2308') and "VisInForm" = 'N')
			       when '43679' then (select count(*) from CPRF where "UserSign" = {1} and "FormID" = '169' and "ItemID" in ('43687', '43688', '43682', '43683') and "VisInForm" = 'N')				   
				 end, 0) when 3 then 'N' else 'Y' end
from dummy		