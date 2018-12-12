select
case {0} 
when 257
then (select {1} from ONCM where AbsEntry = {2}
when 177
then (select {1} from OAGP where AgentCode = '{2}')
when 53
then (select {1} from OSLP where SlpCode = {2})
end