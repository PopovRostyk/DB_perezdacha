use ucubot;
CREATE TABLE student(id int not null auto_increment,
    first_name varchar(255),
    last_name varchar(255),
	user_id varchar(255) unique,
	primary key(id)
);