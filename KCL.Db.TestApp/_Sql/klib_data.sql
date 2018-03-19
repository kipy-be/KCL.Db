-- Authors

    INSERT INTO authors (nick, firstname, lastname)
    VALUES ('kipy', NULL, NULL),
           ('pat',  'Patrick', 'Bond'),
           ('jb',   'Jean',    'Bon');
    
-- Articles

    INSERT INTO articles (author_id, title, content)
    VALUES ((SELECT id FROM authors WHERE nick = 'kipy'), 'First !',   'Bla bla bla'),
           ((SELECT id FROM authors WHERE nick = 'kipy'), 'Deuusss !', 'Bla bla bla yoplaboum'),
           ((SELECT id FROM authors WHERE nick = 'pat'),  'Hi !',      'Cia');
    
-- Comments

    INSERT INTO comments (article_id, author_id, content)
    VALUES ((SELECT id FROM articles WHERE title = 'First !'), (SELECT id FROM authors WHERE nick = 'pat'), 'Great news bud !'),
           ((SELECT id FROM articles WHERE title = 'First !'), (SELECT id FROM authors WHERE nick = 'jb'),  'Indeed !'),
           ((SELECT id FROM articles WHERE title = 'Hi !'),    (SELECT id FROM authors WHERE nick = 'jb'),  'Lol'),
           ((SELECT id FROM articles WHERE title = 'Hi !'),    (SELECT id FROM authors WHERE nick = 'pat'),  'Yeah.');
