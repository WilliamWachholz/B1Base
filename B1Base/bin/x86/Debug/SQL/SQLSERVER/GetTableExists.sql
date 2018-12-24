select isnull(
(select case table_name when '@{0}' then 1 when '{0}' then 2 end
from information_schema.tables
where table_name = '@{0}' or table_name = '{0}'), 0)