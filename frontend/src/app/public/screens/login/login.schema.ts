import z from 'zod';

export const LoginSchema = z.object({
    username: z
        .string()
        .min(6, { message: 'Username must be at least 6 characters' }),
    password: z
        .string()
        .min(6, { message: 'Password must be at least 6 characters' }),
});
