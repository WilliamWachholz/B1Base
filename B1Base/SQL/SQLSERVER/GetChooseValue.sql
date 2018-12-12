select
case {0} 
when 257
then (select NcmCode from ONCM where AbsEntry = {1})
when 177
then (select AgentName from OAGP where AgentCode = '{1}')
when 53
then (select SlpName from OSLP where SlpCode = {1})
end