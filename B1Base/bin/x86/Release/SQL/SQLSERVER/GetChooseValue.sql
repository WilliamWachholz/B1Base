select
case {0} 
when 2
then (select CardName from OCRD where CardCode = '{1}')
when 257
then (select NcmCode from ONCM where AbsEntry = '{1}')
when 177
then (select AgentName from OAGP where AgentCode = '{1}')
when 53
then (select SlpName from OSLP where SlpCode = '{1}')
when 140000041
then (select DNFCode from ODNF where AbsEntry = '{1}')
when 256
then (select MatGrp from OMGP where AbsEntry = '{1}')
when 200
then (select descript from OTER where territryID = '{1}')
end