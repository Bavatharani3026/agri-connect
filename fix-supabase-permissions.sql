-- ====================================================================
-- AgriConnect: Fix permissions for platform_users in Supabase
-- Run this script in your Supabase Dashboard -> SQL Editor
-- ====================================================================

-- Step 1: Grant table permissions to service_role, authenticated, and anon
GRANT ALL ON TABLE public.platform_users TO postgres, service_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.platform_users TO authenticated;
GRANT SELECT ON TABLE public.platform_users TO anon;

-- Step 2: Grant sequence usage (if any sequence exists in public)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO postgres, service_role, authenticated;

-- Step 3: Configure Row Level Security (RLS)
ALTER TABLE public.platform_users ENABLE ROW LEVEL SECURITY;

-- Policy 1: Service role has unrestricted access (used by backend Admin Service)
DROP POLICY IF EXISTS "service_role_all" ON public.platform_users;
CREATE POLICY "service_role_all" ON public.platform_users
  FOR ALL
  TO service_role
  USING (true)
  WITH CHECK (true);

-- Policy 2: Users can view their own profile
DROP POLICY IF EXISTS "users_read_own" ON public.platform_users;
CREATE POLICY "users_read_own" ON public.platform_users
  FOR SELECT
  TO authenticated
  USING (auth.uid() = auth_user_id);

-- Policy 3: Users can update their own profile
DROP POLICY IF EXISTS "users_update_own" ON public.platform_users;
CREATE POLICY "users_update_own" ON public.platform_users
  FOR UPDATE
  TO authenticated
  USING (auth.uid() = auth_user_id)
  WITH CHECK (auth.uid() = auth_user_id);

-- Verification: confirm table grants
SELECT grantee, privilege_type 
FROM information_schema.role_table_grants 
WHERE table_name = 'platform_users';
