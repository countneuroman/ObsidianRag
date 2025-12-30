FROM postgres:17

RUN apt update \
 && apt install -y postgresql-17-pgvector \
 && rm -rf /var/lib/apt/lists/*

COPY init.sql /docker-entrypoint-initdb.d/init.sql