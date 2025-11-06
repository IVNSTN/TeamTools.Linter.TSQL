CREATE TABLE bar.foo
(
    id           INT         NOT NULL
    , parent_id  INT         NOT NULL
    , type_code  VARCHAR(20) NOT NULL
    , subtype_id SMALLINT    NOT NULL
    , inline_id  INT         NOT NULL CONSTRAINT FK_bar_foo FOREIGN KEY REFERENCES jar.mar (inline_fk_id)                  -- no col
    , CONSTRAINT FK_foo_parent_id FOREIGN KEY (parent_id) REFERENCES zar.far (self_id)                                     -- no schema
    , CONSTRAINT FrgnKey_bar_foo_parent_id FOREIGN KEY (parent_id) REFERENCES zar.far (self_id)                            -- wrong prefix
    , CONSTRAINT bar_foo_parent_id FOREIGN KEY (parent_id) REFERENCES zar.far (self_id)                                    -- no FK prefix
    , CONSTRAINT FK_bar_foo FOREIGN KEY (parent_id) REFERENCES zar.dar (self_id)                                           -- no column suffix
    , CONSTRAINT FK_bar_foo_parent_id_type_code FOREIGN KEY (type_code, parent_id) REFERENCES zar.far (code_name, self_id) -- wrong col order
);
GO

-- wrong numeronim
ALTER TABLE bar.foo
ADD CONSTRAINT FK_bar_foo_type_code_p777d
    FOREIGN KEY (type_code, parent_id, subtype_id)
    REFERENCES zar.far (code_name, self_id, subtype_id);
GO
