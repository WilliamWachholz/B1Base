select
case {0} 
when 13
then (select  "DocEntry" from OINV with (nolock) where "DocNum" = {1})
when 17
then (select "DocEntry" from ORDR with (nolock) where "DocNum" = {1})
when 18
then (select  "DocEntry" from OPCH with (nolock) where "DocNum" = {1})
when 22
then (select "DocEntry" from OPOR with (nolock) where "DocNum" = {1})
end