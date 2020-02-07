update "OCRD" set "AgentCode" = CASE '{0}' WHEN '' THEN NULL ELSE '{0}' END
where "CardCode" = '{1}'