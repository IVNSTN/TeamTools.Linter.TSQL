CREATE TABLE bar.foo
(
    id           INT         NOT NULL
    , parent_id  INT         NOT NULL
    , type_code  VARCHAR(20) NOT NULL
    , subtype_id SMALLINT    NOT NULL
    , inline_id  INT         NOT NULL CONSTRAINT FK_bar_foo_inline_id FOREIGN KEY REFERENCES jar.mar (inline_fk_id)
    , CONSTRAINT FK_bar_foo_parent_id FOREIGN KEY (parent_id) REFERENCES zar.far (self_id)
    , CONSTRAINT FK_bar_foo_type_code_parent_id FOREIGN KEY (type_code, parent_id) REFERENCES zar.far (code_name, self_id)
);
GO

ALTER TABLE bar.foo
ADD CONSTRAINT FK_bar_foo_type_code_p18d
    FOREIGN KEY (type_code, parent_id, subtype_id)
    REFERENCES zar.far (code_name, self_id, subtype_id);
GO
