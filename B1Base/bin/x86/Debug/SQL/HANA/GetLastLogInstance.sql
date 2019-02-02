select
case {0} 
when 13
then (select "LogInstanc" from "ADOC" where "DocEntry" = {1} and "ObjType" = 13)
when 17
then (select "LogInstanc" from "ADOC" where "DocEntry" = {1} and "ObjType" = 17)
end
from dummy