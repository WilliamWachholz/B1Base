select	"{1}",
		case when length("{2}") > 140 then substring("{2}", 0, 140) else "{2}" end
from {0}
union
select  0,
		''
from dummy