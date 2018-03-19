-- -------------------------------------------------------------------------------------------------------------------------------------------------------
--	Tables
-- -------------------------------------------------------------------------------------------------------------------------------------------------------

-- Authors

    CREATE TABLE authors
    (
        id SERIAL PRIMARY KEY,
        nick VARCHAR(32) NOT NULL,
        firstname CHAR(32),
        lastname CHAR(32)
    );
    
-- News

    CREATE TABLE articles
    (
        id SERIAL PRIMARY KEY,
        author_id INTEGER NOT NULL,
        title VARCHAR(128) NOT NULL,
        content VARCHAR(4096) NOT NULL
    );
    
-- Comments

    CREATE TABLE comments
    (
        id SERIAL PRIMARY KEY,
        article_id INTEGER NOT NULL,
        author_id INTEGER NOT NULL,
        content VARCHAR(2048) NOT NULL
    );

-- -------------------------------------------------------------------------------------------------------------------------------------------------------
--	Foreigns keys / Indexes / Constraints
-- -------------------------------------------------------------------------------------------------------------------------------------------------------


-- Comments

    -- comments <-> articles
    ALTER TABLE comments ADD CONSTRAINT FK_comment_article
        FOREIGN KEY (article_id) REFERENCES articles (id);

    CREATE INDEX IDX_comment_article
        ON comments (article_id);

    -- comment <-> author
    ALTER TABLE comments ADD CONSTRAINT FK_comment_author
        FOREIGN KEY (author_id) REFERENCES authors (id);

    CREATE INDEX IDX_comment_author
        ON comments (author_id);
        
-- News

    -- article <-> author
    ALTER TABLE articles ADD CONSTRAINT FK_article_author
        FOREIGN KEY (author_id) REFERENCES authors (id);

    CREATE INDEX IDX_article_author
        ON articles (author_id);
