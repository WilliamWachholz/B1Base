select
case {0} 
when 2
then (select "CardName" from OCRD where "CardCode" = '{1}')
when 6
then (select "ListName" from OPLN where "ListNum" = '{1}')
when 257
then (select "NcmCode" from ONCM where "AbsEntry" = '{1}')
when 177
then (select "AgentName" from OAGP where "AgentCode" = '{1}')
when 53
then (select "SlpName" from OSLP where "SlpCode" = '{1}')
when 140000041
then (select "DNFCode" from ODNF where "AbsEntry" = '{1}')
when 256
then (select "MatGrp" from OMGP where "AbsEntry" = '{1}')
when 200
then (select "descript" from OTER where "territryID" = '{1}')
when 129
then (select "Name" from OCRY where "Code" = '{1}')
when 130
then (select "Name" from OCST where "Code" = '{1}')
when 265
then (select "Name" from OCNT where "Code" = '{1}')
when 160
then (select "QName" from OUQR where "IntrnalKey" = '{1}')
end
from dummy