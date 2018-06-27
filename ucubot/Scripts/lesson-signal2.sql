use ucubot;
alter table lesson_signal drop user_id;
alter table lesson_signal add student_id int;
alter table lesson_signal add
	constraint fk_student_id
	foreign key (student_id) references student(id)
  on update restrict on delete restrict;