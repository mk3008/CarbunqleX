-- 親の親: Organization
CREATE TABLE organizations (
    organization_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

-- 親: Blog（各Postが所属する）
CREATE TABLE blogs (
    blog_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    organization_id INT REFERENCES organizations(organization_id) ON DELETE CASCADE
);

-- 親: User（投稿者）
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

-- 投稿: Post（起点）
CREATE TABLE posts (
    post_id SERIAL PRIMARY KEY,
    title TEXT NOT NULL,
    content TEXT NOT NULL,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    blog_id INT REFERENCES blogs(blog_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 子: Tags（多対多）
CREATE TABLE tags (
    tag_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE post_tags (
    post_id INT REFERENCES posts(post_id) ON DELETE CASCADE,
    tag_id INT REFERENCES tags(tag_id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, tag_id)
);

-- 子: Comments（投稿に紐づく）
CREATE TABLE comments (
    comment_id SERIAL PRIMARY KEY,
    post_id INT REFERENCES posts(post_id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- 子の子: Likes（コメントに紐づく）
CREATE TABLE likes (
    like_id SERIAL PRIMARY KEY,
    comment_id INT REFERENCES comments(comment_id) ON DELETE CASCADE,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE
);

-- 組織データ
INSERT INTO organizations (name) VALUES ('Tech Corp'), ('Creative Hub');

-- ブログデータ
INSERT INTO blogs (name, organization_id) VALUES 
('AI Insights', 1),
('Design Trends', 2);

-- ユーザーデータ
INSERT INTO users (name) VALUES ('Alice'), ('Bob');

-- 投稿データ
INSERT INTO posts (title, content, user_id, blog_id) VALUES 
('Understanding AI', 'This is a post about AI.', 1, 1),
('Deep Learning Advances', 'Exploring the latest in deep learning.', 1, 1),
('Latest Design Trends', 'Exploring modern design.', 2, 2),
('User Experience Matters', 'How UX shapes our interactions.', 2, 2);

-- タグデータ
INSERT INTO tags (name) VALUES ('AI'), ('Machine Learning'), ('Design'), ('UX');

-- 投稿とタグの関連データ
INSERT INTO post_tags (post_id, tag_id) VALUES 
(1, 1), (1, 2), 
(2, 3), (2, 4);

-- コメントデータ
INSERT INTO comments (post_id, content) VALUES 
(1, 'Great post!'),
(1, 'Very insightful!'),
(2, 'I love these design ideas.');

-- いいねデータ
INSERT INTO likes (comment_id, user_id) VALUES 
(1, 2), 
(2, 1);
