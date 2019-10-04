select
case {0} 
when 13
then (select  "DocEntry" from OINV where "DocNum" = {1})
when 17
then (select "DocEntry" from ORDR where "DocNum" = {1})
when 18
then (select  "DocEntry" from OPCH where "DocNum" = {1})
when 22
then (select "DocEntry" from OPOR where "DocNum" = {1})
end
from dummy