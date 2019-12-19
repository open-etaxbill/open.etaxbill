--
-- This script creates the group OpenETaxBill and the user odinsoft
--

--
-- Create the OpenETaxBill group
--
CREATE GROUP OpenETaxBill;

--
-- Create the odinsoft user with createdb and createuser
-- permissions.  Place the user in the OpenETaxBill group and
-- set the password to the default of odinsoft.
--
CREATE USER odinsoft WITH PASSWORD 'abcde12#'
                       CREATEDB CREATEUSER
                       IN GROUP OpenETaxBill;
