select
case {0} 
when 13
then (select top(1) "LogInstanc" from ADOC with (nolock) where "DocEntry" = {1} and "ObjType" = 13 order by "LogInstanc" desc)
when 17
then (select top(1) "LogInstanc" from ADOC with (nolock) where "DocEntry" = {1} and "ObjType" = 17 order by "LogInstanc" desc)
end