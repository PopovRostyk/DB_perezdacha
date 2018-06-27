use ucubot;
create table lesson_signal(id int not null auto_increment,
 timestamp datetime default current_timestamp,
 signal_type int not null,
 user_id varchar(255),
 primary key(id));